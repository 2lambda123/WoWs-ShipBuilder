﻿using System;
using System.Collections.Generic;
using WoWsShipBuilder.DataStructures;

namespace WoWsShipBuilder.Core.DataContainers;

public static class AccelerationHelper
{
    private const string VermontId = "PASB110";
    private const string MarceuId = "PFSD210";
    private const string CaraccioloId = "PISB107";
    private const double Dt = 0.01;
    const int SailingTime = 3;

    public static AccelerationData CalculateAcceleration(
        string shipIndex,
        Hull hull,
        Engine engine,
        ShipClass shipClass,
        double speedMultiplier = 1,
        double engineForwardUpTimeModifiers = 1,
        double engineBackwardUpTimeModifiers = 1,
        double engineForwardForsageMaxSpeedModifier = 1,
        double engineBackwardForsageMaxSpeedModifier = 1,
        double engineForwardForsagePowerModifier = 1,
        double engineBackwardForsagePowerModifier = 1)
    {
        var result = new List<AccelerationPoints>();

        // get starting stats
        var baseShipSpeed = decimal.ToDouble((1 + engine.SpeedCoef) * hull.MaxSpeed);
        var horsepower = decimal.ToDouble(hull.EnginePower);
        int tonnage = hull.Tonnage;
        var fullPowerForwardTime = decimal.ToDouble(engine.ForwardEngineUpTime);
        var fullPowerBackwardTime = decimal.ToDouble(engine.BackwardEngineUpTime);
        var timeConstant = decimal.ToDouble(Constants.TimeScale);
        var forwardEngineForsag = decimal.ToDouble(engine.ForwardEngineForsag);
        var backwardEngineForsag = decimal.ToDouble(engine.BackwardEngineForsag);
        var forwardEngineForsagMaxSpeed = decimal.ToDouble(engine.ForwardEngineForsagMaxSpeed);
        var backwardEngineForsagMaxSpeed = decimal.ToDouble(engine.BackwardEngineForsagMaxSpeed);

        double speed = 0;
        double time = 0;
        double power = 0;

        // throttle goes from -1 to 4, depending on the gear.
        double throttle = 4;

        // calculate other stats
        var maxForwardSpeed = baseShipSpeed * speedMultiplier;
        var maxReverseSpeed = ((baseShipSpeed / 4) + 4.9) * speedMultiplier;

        var powerRatio = horsepower / tonnage;
        var maxPowerForward = Math.Pow(powerRatio, 0.42) * Math.Pow(speedMultiplier, 2);
        var maxPowerBackwards = (maxPowerForward / GetPfToPbRatio(shipClass, shipIndex)) * Math.Pow(speedMultiplier, 2);

        var timeForward = (fullPowerForwardTime / timeConstant) * engineForwardUpTimeModifiers;
        var timeBackward = (fullPowerBackwardTime / timeConstant) * engineBackwardUpTimeModifiers;

        var forsageForward = forwardEngineForsag * engineForwardForsagePowerModifier;
        var forsageBackwards = backwardEngineForsag * engineBackwardForsagePowerModifier;

        var forsageForwardMaxSpeed = forwardEngineForsagMaxSpeed * engineForwardForsageMaxSpeedModifier;
        var forsageBackwardsMaxSpeed = backwardEngineForsagMaxSpeed * engineBackwardForsageMaxSpeedModifier;

        var powerIncreaseForward = Dt * maxPowerForward / timeForward;
        var powerIncreaseBackward = Dt * maxPowerBackwards / timeBackward;

        // begin the pain, aka the math
        int isDecelerating = 0;

        result.Add(new(speed, time));

        double speedLimit = GetSpeedLimit(throttle, maxForwardSpeed, maxReverseSpeed);

        // we go forward!
        while (speed < maxForwardSpeed - 0.1)
        {
            DoIteration(result, shipIndex, throttle, isDecelerating, ref time, ref speed, speedLimit, ref power, powerIncreaseForward, maxPowerForward, maxForwardSpeed, forsageForwardMaxSpeed, forsageForward, powerIncreaseBackward, maxPowerBackwards, maxReverseSpeed, forsageBackwardsMaxSpeed, forsageBackwards);
        }

        // set the power to max of the current throttle and add a point at max speed. This is done because the speed function is asymptotic to MaxSpeed, so we force it to finish slightly earlier.
        power = (throttle >= 0) ? maxPowerForward * Math.Pow(Math.Pow(throttle / 4, 2), isDecelerating) : maxPowerBackwards;
        result.Add(new(maxForwardSpeed, time += Dt));
        double timeToFullForward = time;

        // and now we sail for a bit and then stop!
        bool decelerate = false;
        if (decelerate)
        {
            // Add some sailing time to have a straight line before decelerating
            time += SailingTime;
            result.Add(new (speed, time));
            throttle = 0;
            speedLimit = GetSpeedLimit(throttle, maxForwardSpeed, maxReverseSpeed);

            while (speed > 0)
            {
                DoIteration(result, shipIndex, throttle, isDecelerating, ref time, ref speed, speedLimit, ref power, powerIncreaseForward, maxPowerForward, maxForwardSpeed, forsageForwardMaxSpeed, forsageForward, powerIncreaseBackward, maxPowerBackwards, maxReverseSpeed, forsageBackwardsMaxSpeed, forsageBackwards);
            }
        }

        double timeToFullBackward = time;

        return new(timeToFullForward, timeToFullBackward, result);
    }

    // this ratio is about 4 for battle (exception is 3 for Caracciolo), 3 for cruiser, and 2 for expulsion.
    // add exception for caraccioulo
    private static int GetPfToPbRatio(ShipClass shipClass, string shipIndex)
    {
        if (shipIndex.Equals(CaraccioloId))
        {
            return 3;
        }

        switch (shipClass)
        {
            case ShipClass.Battleship:
                return 4;
            case ShipClass.Cruiser:
                return 3;
            case ShipClass.Destroyer:
                return 2;
            default:
                return 4;
        }
    }

    // Calculate water resistance
    private static double GetDrag(double speed, double maxForwardSpeed, double maxPowerForward, double maxReverseSpeed, double maxPowerBackwards)
    {
        // drag=@(x) (-x.*abs(x)/(Vmax)^2*PF).*(x>0) + (-x.*abs(x)/(Vmin)^2*PB).*(x<0);
        // first part if x > 0, second if x < 0
        double result;
        if (speed > 0)
        {
            double speedRatio = (-speed * Math.Abs(speed)) / Math.Pow(maxForwardSpeed, 2);
            result = speedRatio * maxPowerForward;
        }
        else
        {
            double speedRatio = (-speed * Math.Abs(speed)) / Math.Pow(maxReverseSpeed, 2);
            result = -speedRatio * maxPowerBackwards;
        }

        return result;
    }

    private static double GetSpeedLimit(double throttle, double maxForwardSpeed, double maxReverseSpeed)
    {
        // speed_limit=@(x) x/4*Vmax*(x>=0)-Vmin*(x<0);
        // first part if x > 0, second if x < 0
        double result;
        if (throttle >= 0)
        {
            result = throttle / 4 * maxForwardSpeed;
        }
        else
        {
            result = -maxReverseSpeed;
        }

        return result;
    }

    private static void DoIteration(
        List<AccelerationPoints> pointList,
        string shipIndex,
        double throttle,
        double isDecelerating,
        ref double time,
        ref double speed,
        double speedLimit,
        ref double power,
        double powerIncreaseForward,
        double maxPowerForward,
        double maxForwardSpeed,
        double forsageForwardMaxSpeed,
        double forsageForward,
        double powerIncreaseBackward,
        double maxPowerBackwards,
        double maxReverseSpeed,
        double forsageBackwardsMaxSpeed,
        double forsageBackwards)
    {
        int acc;
        if (speedLimit > speed)
        {
            power = shipIndex.Equals(VermontId) || shipIndex.Equals(MarceuId) ? maxPowerForward : Math.Min(Math.Max(power, 0) + powerIncreaseForward, maxPowerForward * Math.Pow(Math.Pow(throttle / 4, 2), isDecelerating));

            acc = 1;
        }
        else if (speedLimit < speed)
        {
            power = Math.Max(Math.Min(power, 0) - powerIncreaseBackward, -maxPowerBackwards);
            acc = -1;
        }
        else
        {
            if (speed > 0)
            {
                power = maxPowerForward * Math.Pow(throttle / 4, 2);
            }
            else
            {
                power = -maxPowerBackwards;
            }

            acc = 0;
        }

        double drag = GetDrag(speed, maxForwardSpeed, maxPowerForward, maxReverseSpeed, maxPowerBackwards);
        double acceleration = (power + drag) * Math.Abs(acc);

        // apply mods
        if (speed < forsageForwardMaxSpeed && speed >= 0 && power > 0)
        {
            acceleration = (maxPowerForward * forsageForward) - drag;
        }

        if (speed > -forsageBackwardsMaxSpeed && speed <= 0 && power < 0)
        {
            acceleration = (-maxPowerBackwards * forsageBackwards) + drag;
        }

        double previousSpeed = speed;

        speed += Dt * acceleration;

        if (speedLimit < speed && acc == 1 && power * previousSpeed > 0)
        {
            speed = speedLimit;
        }

        if (speedLimit > speed && acc == -1 && power * previousSpeed > 0)
        {
            speed = speedLimit;
        }

        time += Dt;

        pointList.Add(new (speed, time));
    }

    public sealed record AccelerationPoints(double Speed, double Time);

    public sealed record AccelerationData(double TimeToFullSpeedForward, double TimeToFullSpeedBackward, List<AccelerationPoints> AccelerationPointsList);
}

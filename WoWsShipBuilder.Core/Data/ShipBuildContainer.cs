﻿using System;
using System.Collections.Generic;
using System.Linq;
using WoWsShipBuilder.Core.Builds;
using WoWsShipBuilder.Core.DataContainers;
using WoWsShipBuilder.DataStructures.Ship;

namespace WoWsShipBuilder.Core.Data;

public sealed record ShipBuildContainer(Ship Ship, Build? Build, Guid Id, IEnumerable<int>? ActivatedConsumableSlots, bool SpecialAbilityActive, ShipDataContainer? ShipDataContainer, List<(string, float)>? Modifiers)
{
    public static ShipBuildContainer CreateNew(Ship ship, Build? build, IEnumerable<int>? activatedConsumableSlots, bool specialAbilityActive = false)
    {
        return new(ship, build, Guid.NewGuid(), activatedConsumableSlots, specialAbilityActive, null, null);
    }

    /// <summary>
    /// Compares this ShipBuilderContainer with another object and determines whether their value is equivalent.
    /// <br/>
    /// Equivalence is determined based on the <see cref="Ship"/>, <see cref="Build"/>, <see cref="ActivatedConsumableSlots"/> and <see cref="SpecialAbilityActive"/> properties.
    /// <br/>
    /// The <see cref="ShipDataContainer"/> and <see cref="Modifiers"/> properties are ignored because they are generated based on the other values and therefore not needed for an equivalence check.
    /// <br/>
    /// The <see cref="Guid"/> property is also irrelevant because two different records can have the same configuration.
    /// </summary>
    /// <param name="newContainer">The other object to compare with this record.</param>
    /// <returns><see langword="true"/> if all relevant properties have the same value, <see langword="false"/> otherwise.</returns>
    public bool IsEquivalentTo(ShipBuildContainer newContainer)
    {
        if (Ship.Index != newContainer.Ship.Index)
        {
            return false;
        }

        if (Build is null && newContainer.Build is not null)
        {
            return false;
        }

        if (Build is not null && !Build.Equals(newContainer.Build))
        {
            return false;
        }

        if (SpecialAbilityActive != newContainer.SpecialAbilityActive)
        {
            return false;
        }

        if ((ActivatedConsumableSlots is null && newContainer.ActivatedConsumableSlots is not null) || (ActivatedConsumableSlots is not null && newContainer.ActivatedConsumableSlots is null))
        {
            return false;
        }

        IOrderedEnumerable<int> oldConsumables = (ActivatedConsumableSlots ?? Enumerable.Empty<int>()).OrderBy(i => i);
        IOrderedEnumerable<int> newConsumables = (newContainer.ActivatedConsumableSlots ?? Enumerable.Empty<int>()).OrderBy(i => i);
        return oldConsumables.SequenceEqual(newConsumables);
    }
}

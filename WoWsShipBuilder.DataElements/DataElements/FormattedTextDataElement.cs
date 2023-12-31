﻿using System.Collections.Generic;

namespace WoWsShipBuilder.DataElements.DataElements;

/// <summary>
/// A record that represent a formatted text data element.
/// </summary>
/// <param name="Text">The text to format. Need to respect <see cref="string.Format(string,object[])"/> specifications.</param>
/// <param name="Values">The enumerable that will be used by <see cref="string.Format(string,object[])"/>.</param>
/// <param name="IsTextKey">If <see cref="Text"/> is a localization key and not actual text.</param>
/// <param name="AreValuesKeys">If <see cref="Values"/> are localization keys and not actual values.</param>
public sealed record FormattedTextDataElement(string Text, IEnumerable<string> Values, bool IsTextKey = false, bool IsTextAppLocalization = false, bool AreValuesKeys = false, bool AreValuesAppLocalization = false) : IDataElement;

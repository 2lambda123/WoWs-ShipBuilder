﻿using System;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
using Newtonsoft.Json;
using WoWsShipBuilder.Core.Services;

// ReSharper disable VirtualMemberNeverOverridden.Global
namespace WoWsShipBuilder.Core.HttpClients;

public abstract class ClientBase
{
    protected ClientBase(IDataService dataService, IAppDataService appDataService)
    {
        DataService = dataService;
        AppDataService = appDataService;
    }

    protected IAppDataService AppDataService { get; }

    protected IDataService DataService { get; }

    protected abstract HttpClient Client { get; }

    internal virtual async Task DownloadFileAsync(Uri uri, string fileName)
    {
        await using Stream stream = await Client.GetStreamAsync(uri);
        await DataService.StoreAsync(stream, fileName);
    }

    internal virtual async Task<T?> GetJsonAsync<T>(string url, JsonSerializer? customSerializer = null)
    {
        await using Stream stream = await Client.GetStreamAsync(url);
        return GetJson<T>(stream, customSerializer);
    }

    internal T? GetJson<T>(Stream stream, JsonSerializer? customSerializer = null)
    {
        using var streamReader = new StreamReader(stream);
        using var jsonReader = new JsonTextReader(streamReader);
        JsonSerializer serializer = customSerializer ?? new JsonSerializer();
        return serializer.Deserialize<T>(jsonReader);
    }
}

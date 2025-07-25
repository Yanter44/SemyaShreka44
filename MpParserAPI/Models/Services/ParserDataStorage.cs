﻿using MpParserAPI.Interfaces;
using MpParserAPI.Models;
using System.Collections.Concurrent;
using TL;
using WTelegram;

namespace MpParserAPI.Services
{
    public  class ParserDataStorage : IParserDataStorage
    {
        private readonly ConcurrentDictionary<Guid, ParserData> _parsers = new();
     
        private readonly ConcurrentDictionary<Guid, TemporaryAuthData> _temporaryauthData = new(); //parserId - tempAuthData
        private readonly ConcurrentDictionary<Guid, Client> _tempClients = new();
        private readonly Dictionary<Guid, Func<IObject, Task>> _handlers = new();

        public Client GetTempClient(Guid TempAuthId)
        {
            return _tempClients[TempAuthId];
        }
        public void AddTempClient(Guid tempAuthId, Client client)
        {
            _tempClients[tempAuthId] = client;
        }
        public bool TryGetTempClient(Guid tempAuthId, out Client client)
        {
            return _tempClients.TryGetValue(tempAuthId, out client);
        }
        public void RemoveTempClient(Guid tempAuthId)
        {
            if (_tempClients.TryRemove(tempAuthId, out var client))
            {
                client.Dispose();
            }
        }
        public void AddOrUpdateParser(Guid parserId, ParserData data)
        {
            _parsers[parserId] = data;
        }
        public ICollection<ParserData> GetAllParsers()
        {
            return _parsers.Values;
        }
        public bool TryGetParser(Guid parserId, out ParserData data)
        {
            return _parsers.TryGetValue(parserId, out data);
        }
        public ParserData GetParser(Guid parserId)
        {
            _parsers.TryGetValue(parserId, out var parser);
            return parser;
        }

        public void TryRemoveParser(Guid parserId)
        {
            _parsers.TryRemove(parserId, out _);
            _temporaryauthData.TryRemove(parserId, out _);
        }
        public bool ContainsParser(Guid parserId)
        {
            return _parsers.ContainsKey(parserId);
        }
        public void AddOrUpdateTemporaryAuthData(Guid parserId, TemporaryAuthData temporarydata)
        {
            _temporaryauthData[parserId] = temporarydata;
        }
        public bool TryGetTemporaryAuthData(Guid parserId, out TemporaryAuthData tempData)
        {
            return _temporaryauthData.TryGetValue(parserId, out tempData);
        }
        public TemporaryAuthData GetOrCreateTemporaryAuthData(Guid parserId)
        {
            return _temporaryauthData.GetOrAdd(parserId, _ => new TemporaryAuthData());
        }
        public void TryRemoveTemporaryAuthData(Guid parserId)
        {
            _temporaryauthData.TryRemove(parserId, out _);
        }

        public void AddHandler(Guid parserId, Func<IObject, Task> handler)
        {
            _handlers[parserId] = handler; 
        }

        public bool TryGetHandler(Guid parserId, out Func<IObject, Task> handler)
        {

            return _handlers.TryGetValue(parserId, out handler);
        }

        public void RemoveHandler(Guid parserId)
        {
            if (_handlers.TryGetValue(parserId, out var handler))
            {
                _handlers.Remove(parserId);
            }
        }
        public bool TryGetTempClientByPhone(string phone, out Client client)
        {
            client = null;

            var tempEntry = _temporaryauthData
                .FirstOrDefault(kvp => kvp.Value.Phone == phone);

            if (tempEntry.Key != Guid.Empty && _tempClients.TryGetValue(tempEntry.Key, out client))
            {
                return true;
            }

            return false;
        }

        public void RemoveTempClientByPhone(string phone)
        {
            var tempEntry = _temporaryauthData
                .FirstOrDefault(kvp => kvp.Value.Phone == phone);

            if (tempEntry.Key != Guid.Empty)
            {
                RemoveTempClient(tempEntry.Key); 
                _temporaryauthData.TryRemove(tempEntry.Key, out _);
            }
        }

    }
}

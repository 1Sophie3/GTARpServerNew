using System.Collections.Generic;

namespace RPCore.Services
{
    public class BankService
    {
        private readonly Dictionary<string, int> _accounts = new(); // uuid -> Kontostand

        public (bool Success, int NewBalance) Deposit(string uuid, int amount)
        {
            if (amount <= 0) return (false, GetBalance(uuid));
            if (!_accounts.ContainsKey(uuid)) _accounts[uuid] = 0;
            _accounts[uuid] += amount;
            return (true, _accounts[uuid]);
        }

        public (bool Success, int NewBalance) Withdraw(string uuid, int amount)
        {
            if (amount <= 0 || !_accounts.ContainsKey(uuid) || _accounts[uuid] < amount)
                return (false, GetBalance(uuid));
            _accounts[uuid] -= amount;
            return (true, _accounts[uuid]);
        }

        public (bool Success, int FromBalance, int ToBalance) Transfer(string fromUuid, string toUuid, int amount)
        {
            if (amount <= 0 || !_accounts.ContainsKey(fromUuid) || _accounts[fromUuid] < amount)
                return (false, GetBalance(fromUuid), GetBalance(toUuid));
            if (!_accounts.ContainsKey(toUuid)) _accounts[toUuid] = 0;
            _accounts[fromUuid] -= amount;
            _accounts[toUuid] += amount;
            return (true, _accounts[fromUuid], _accounts[toUuid]);
        }

        public int GetBalance(string uuid)
        {
            return _accounts.TryGetValue(uuid, out var bal) ? bal : 0;
        }
    }
}

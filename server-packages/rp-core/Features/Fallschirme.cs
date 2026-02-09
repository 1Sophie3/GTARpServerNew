#nullable enable
using GTANetworkAPI;
using System.Collections.Generic;
using System;
using System.Threading;

namespace RPCCore.Features
{
    // Datencontainer-Klasse für einen einzelnen Fallschirm-Spawn
    public class ParachuteSpawn
    {
        public Vector3 Position { get; }
        public GTANetworkAPI.Object? Object { get; set; }
        public TextLabel? Label { get; set; }
        public bool IsTaken { get; set; } = false;
        public Timer? RespawnTimer { get; set; }

        public ParachuteSpawn(Vector3 position)
        {
            Position = position;
        }
    }

    public class FallschirmWorkaround : Script
    {
        // 1. Die öffentliche Instanz, damit spawnconfig.cs darauf zugreifen kann.
        public static FallschirmWorkaround? Instance { get; private set; }

        private readonly List<ParachuteSpawn> _allParachuteSpawns = new List<ParachuteSpawn>();
        private readonly Dictionary<Player, DateTime> _playerCooldown = new Dictionary<Player, DateTime>();

        // 2. Der Konstruktor, der die Instanz für RAGE bekannt macht.
        public FallschirmWorkaround()
        {
            Instance = this;
  
        }

        // 3. Die öffentliche Methode, die du von spawnconfig.cs aufrufen wirst.
        // KEIN [ServerEvent] hier!
        public void CreateAllParachuteSpawns()
        {
            var spawnPositions = new List<Vector3>
            {
                new Vector3(1662.72, -28.91, 182.77),   // Dein erster Fallschirm
                new Vector3(453.87, 5588.94, 781.20)     // Dein zweiter Fallschirm
            };

            foreach (var pos in spawnPositions)
            {
                var newSpawn = new ParachuteSpawn(pos);
                _allParachuteSpawns.Add(newSpawn);
                SpawnSingleParachute(newSpawn);
            }

        }

        private void SpawnSingleParachute(ParachuteSpawn spawn)
        {
            spawn.Object = NAPI.Object.CreateObject(NAPI.Util.GetHashKey("p_parachute_s"), spawn.Position - new Vector3(0, 0, 0.5f), new Vector3(0, 0, 179.55), 255, 0);
            spawn.Label = NAPI.TextLabel.CreateTextLabel("Drücke ~y~E~w~, um den Fallschirm aufzuheben", spawn.Position + new Vector3(0, 0, 0.5), 10.0f, 0.5f, 4, new Color(255, 255, 255, 255), false, 0);
            spawn.IsTaken = false;
        }

        // 4. Der Event-Handler, der jetzt wieder funktioniert, weil die Instanz existiert.
        [RemoteEvent("OnPlayerPressE_Parachute")]
        public void OnPlayerPressE(Player player)
        {
            foreach (var spawn in _allParachuteSpawns)
            {
                if (!spawn.IsTaken && player.Position.DistanceTo(spawn.Position) < 6.0f)
                {
                    if (_playerCooldown.ContainsKey(player) && DateTime.Now < _playerCooldown[player])
                    {
                        player.SendNotification("~r~Du kannst noch keinen neuen Fallschirm aufheben.");
                        return;
                    }

                    player.GiveWeapon(WeaponHash.Parachute, 1);
                    player.SendNotification("~g~Fallschirm aufgehoben!");
                    _playerCooldown[player] = DateTime.Now.AddMinutes(2);

                    spawn.IsTaken = true;
                    spawn.Object?.Delete();
                    spawn.Label?.Delete();

                    spawn.RespawnTimer = new Timer((_) => {
                        NAPI.Task.Run(() => {
                            SpawnSingleParachute(spawn);
                        });
                    }, null, 60000, Timeout.Infinite);

                    return;
                }
            }
        }
    }
}
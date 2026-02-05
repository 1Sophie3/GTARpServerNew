using System;
using System.Collections.Generic;

namespace GTANetworkAPI
{
    // ==================== CORE CLASSES ====================

    public abstract class Script { }

    public class CommandAttribute : Attribute
    {
        public CommandAttribute(string name) { }
    }

    public class ServerEventAttribute : Attribute
    {
        public ServerEventAttribute(Event eventType) { }
    }

    // ==================== ENUMS ====================

    public enum Event
    {
        ResourceStart,
        ResourceStop,
        PlayerConnected,
        PlayerDisconnected,
        PlayerDeath
    }

    public enum DisconnectionType
    {
        Left,
        Timeout,
        Kicked
    }

    public enum VehicleHash : uint
    {
        Adder = 0x63ABADE7,
        Shotaro = 0xF9300668
    }

    public enum WeaponHash : uint
    {
        Pistol = 0x1B06D571,
        Pistol50 = 0x99AEEB3B
    }

    // ==================== ENTITY CLASSES ====================

    public class Player
    {
        public string Name { get; set; } = "";
        public string SocialClubName { get; set; } = "";
        public string Address { get; set; } = "127.0.0.1";
        public string Serial { get; set; } = "STUB-SERIAL";
        public Vector3 Position { get; set; } = new Vector3(0, 0, 0);
        public Vector3 Rotation { get; set; } = new Vector3(0, 0, 0);
        public int Health { get; set; } = 100;
        public int Armor { get; set; } = 0;
        public uint Dimension { get; set; } = 0;
        public bool Exists { get; set; } = true;
        public bool IsInVehicle { get; set; } = false;
        public Vehicle Vehicle { get; set; } = null!;

        public void SendChatMessage(string message) { }
        public void Kick(string reason) { }
        public void Spawn(Vector3 position) { }
        public void TriggerEvent(string eventName, params object[] args) { }
        public void SetSharedData(string key, object value) { }
        public void ResetSharedData(string key) { }
        public void GiveWeapon(WeaponHash weapon, int ammo) { }
        public void RemoveAllWeapons() { }
        public void SetData(string key, object value) { }
        public T GetData<T>(string key) => default(T)!;
        public void SetIntoVehicle(Vehicle vehicle, int seat) { }
    }

    public class Vehicle
    {
        public Vector3 Position { get; set; } = new Vector3(0, 0, 0);
        public Vector3 Rotation { get; set; } = new Vector3(0, 0, 0);
        public uint Dimension { get; set; } = 0;
        public bool Exists { get; set; } = true;
        public string NumberPlate { get; set; } = "STUB";
        public bool Locked { get; set; } = false;
        public bool EngineStatus { get; set; } = false;

        public void Delete() { }
    }

    public struct Vector3
    {
        public float X { get; set; }
        public float Y { get; set; }
        public float Z { get; set; }

        public Vector3(float x, float y, float z)
        {
            X = x;
            Y = y;
            Z = z;
        }
    }

    // ==================== NAPI STATIC CLASSES ====================

    public static class NAPI
    {
        public static class Util
        {
            public static void ConsoleOutput(string text) { }
            public static uint GetHashKey(string input) => 0;
            public static VehicleHash VehicleNameToModel(string vehicleName) => VehicleHash.Adder;
        }

        public static class Server
        {
            public static void SetAutoSpawnOnConnect(bool enabled) { }
            public static void SetAutoRespawnAfterDeath(bool enabled) { }
            public static void SetDefaultSpawnLocation(Vector3 position) { }
        }

        public static class Chat
        {
            public static void SendChatMessageToAll(string message) { }
            public static void SendChatMessageToPlayer(Player player, string message) { }
        }

        public static class ClientEvent
        {
            public static void TriggerClientEvent(Player player, string eventName, params object[] args) { }
            public static void TriggerClientEventForAll(string eventName, params object[] args) { }
        }

        public static class Pools
        {
            public static List<GTANetworkAPI.Player> GetAllPlayers() => new List<GTANetworkAPI.Player>();
            public static List<GTANetworkAPI.Vehicle> GetAllVehicles() => new List<GTANetworkAPI.Vehicle>();
        }

        public class PlayerMethods
        {
            public void SetPlayerArmor(GTANetworkAPI.Player player, int armor) { }
            public void SetPlayerIntoVehicle(GTANetworkAPI.Player player, GTANetworkAPI.Vehicle vehicle, int seat) { }
            public bool IsPlayerInAnyVehicle(GTANetworkAPI.Player player) => false;
            public GTANetworkAPI.Vehicle GetPlayerVehicle(GTANetworkAPI.Player player) => null!;
            public void GivePlayerWeapon(GTANetworkAPI.Player player, string weaponName, int ammo) { }
            public void GivePlayerWeapon(GTANetworkAPI.Player player, WeaponHash weapon, int ammo) { }
            public void RemoveAllPlayerWeapons(GTANetworkAPI.Player player) { }
        }
        public static PlayerMethods Player = new PlayerMethods();

        public class VehicleMethods
        {
            public GTANetworkAPI.Vehicle CreateVehicle(VehicleHash model, Vector3 position, float heading, int color1, int color2) => new GTANetworkAPI.Vehicle();
            public GTANetworkAPI.Vehicle CreateVehicle(uint hash, Vector3 position, float heading, int color1, int color2) => new GTANetworkAPI.Vehicle();
            public GTANetworkAPI.Vehicle CreateVehicle(string vehicleName, Vector3 position, float heading, int color1, int color2) => new GTANetworkAPI.Vehicle();
        }
        public static VehicleMethods Vehicle = new VehicleMethods();

        public static class Task
        {
            public static void Run(Action action, int delayTime = 0) { }
        }
    }
}


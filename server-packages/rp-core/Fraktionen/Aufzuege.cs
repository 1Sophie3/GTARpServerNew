using System;
using System.Collections.Generic;
using GTANetworkAPI;

public class ElevatorManager : Script
{
    private readonly List<Elevator> elevators = new List<Elevator>();
    private readonly HashSet<Player> cooldownPlayers = new HashSet<Player>();

    public ElevatorManager()
    {
        // Beispiel: Fahrstühle hinzufügen
        AddElevator(new Vector3(306.996, -591.762, 43.271), new Vector3(305.565, -591.328, 47.276), new Vector3(0, 0, 69.299));
        AddElevator(new Vector3(307.100, -591.990, 47.276), new Vector3(305.585, -591.378, 43.270), new Vector3(0, 0, 73.929));
        AddElevator(new Vector3(329.847, -578.918, 74.180), new Vector3(305.565, -591.328, 47.276), new Vector3(0, 0, 69.299));
        AddElevator(new Vector3(306.074, -594.641, 43.271), new Vector3(331.332, -579.4678,74.180), new Vector3(0, 0, -112.746));

    }

    public void AddElevator(Vector3 entryPosition, Vector3 targetPosition, Vector3 targetRotation)
    {
        ColShape colShape = NAPI.ColShape.CreateCylinderColShape(entryPosition, 1.0f, 2.0f, 0);
        colShape.SetData("TargetPosition", targetPosition);
        colShape.SetData("TargetRotation", targetRotation);
        elevators.Add(new Elevator(entryPosition, targetPosition, targetRotation, colShape));
    }

    [ServerEvent(Event.PlayerEnterColshape)]
    public void OnPlayerEnterColshape(ColShape colShape, Player player)
    {
        if (!colShape.HasData("TargetPosition") || !colShape.HasData("TargetRotation") || cooldownPlayers.Contains(player)) return;

        Vector3 targetPosition = colShape.GetData<Vector3>("TargetPosition");
        Vector3 targetRotation = colShape.GetData<Vector3>("TargetRotation");

        // Teleportiere den Spieler mit Rotation
        player.Position = targetPosition;
        player.Rotation = targetRotation; // Setzt die Rotation des Spielers
        player.SendNotification("Du wurdest mit dem Fahrstuhl teleportiert!");

        // Spieler in den Cooldown versetzen
        cooldownPlayers.Add(player);
        NAPI.Task.Run(() =>
        {
            cooldownPlayers.Remove(player);
            player.SendNotification("Du kannst den Fahrstuhl wieder nutzen.");
        }, delayTime: 3000); // 3 Sekunden Cooldown
    }
}

public class Elevator
{
    public Vector3 EntryPosition { get; }
    public Vector3 TargetPosition { get; }
    public Vector3 TargetRotation { get; }
    public ColShape ColShape { get; }

    public Elevator(Vector3 entryPosition, Vector3 targetPosition, Vector3 targetRotation, ColShape colShape)
    {
        EntryPosition = entryPosition;
        TargetPosition = targetPosition;
        TargetRotation = targetRotation;
        ColShape = colShape;
    }
}

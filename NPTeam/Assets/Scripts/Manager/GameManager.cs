using System.Collections.Generic;
using UnityEngine;

public class GameManager : SingletonMonoBehaviour<GameManager>
{
    //각 PC마다 들고 있을 차량.
    [SerializeField] private AssembleController _LeaderVehicle;

    public AssembleController LeaderVehicle
    {
        get => _LeaderVehicle;
        set => _LeaderVehicle = value;
    }

    private Dictionary<int, PlayerVehicle> PlayerVehicles = new();

    public PlayerVehicle GetVehicle(int num)
    {
        PlayerVehicle vehicle = null;
        if (PlayerVehicles.TryGetValue(num, out vehicle))
        {
            return vehicle;
        }

        return vehicle;
    }

    public void SetVehicle(int num, PlayerVehicle vehicle)
    {
        if (!PlayerVehicles.ContainsKey(num))
            PlayerVehicles[num] = vehicle;
        else
            Debug.LogError("중복된 차량 추가");
    }
}
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
    public int GetVehiclesNum => PlayerVehicles.Count;

    public PlayerVehicle GetVehicle(int num)
    {
        if (PlayerVehicles.ContainsKey(num))
        {
            return PlayerVehicles[num];
        }

        return null;
    }

    public void SetVehicle(int num, PlayerVehicle vehicle)
    {
        if (!PlayerVehicles.ContainsKey(num))
            PlayerVehicles[num] = vehicle;
        else
            Debug.LogError("중복된 차량 추가");
    }
}
using System.Collections;
using System.Collections.Generic;
using Player;
using Resources.Rooms;
using UnityEngine;
using UnityEngine.Serialization;

public class MapGenerator : MonoBehaviour
{
    [SerializeField]private GameObject gridObj;
    [SerializeField]private PlayerObj player;
    [SerializeField] public List<GameObject> roomsPrefabs;
    [SerializeField] public List<RoomGenerator> generatedRooms;
    [SerializeField] public List<GameObject> corridorPrefabs;
    [SerializeField] public GameObject deadEnd;
    [SerializeField]private GameObject startRoom;
    private static System.Random random = new System.Random();

    // Start is called before the first frame update
    private void Start()
    {
        var spawnedRoom = Instantiate(startRoom, gridObj.transform);
        generatedRooms.Add(spawnedRoom.GetComponent<RoomGenerator>());
        spawnedRoom.GetComponent<RoomGenerator>().GenerateAdjacentRooms();
    }

    public void Spawn()
    {
        for (var index = 0; index < generatedRooms.Count; index++)
        {
            var generatedRoom = generatedRooms[index];
            generatedRoom.GenerateAdjacentRooms();
        }
    }

    public GameObject InstantiateRandomRoom()
    {
        var randomRoomIndex = random.Next(roomsPrefabs.Count - 1);
        var room = Instantiate(roomsPrefabs[randomRoomIndex]);
        generatedRooms.Add(room.GetComponent<RoomGenerator>());

        return room;
    }
}
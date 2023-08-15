using UnityEngine;

namespace GridPlacement.PlaceTypes
{
    public interface IPlaceType
    {
        void OnClickTriggered(Vector3Int currentGridPosition);
        void OnClickReleased(Vector3Int currentGridPosition);
        void OnUpdate(Vector3Int? currentGridPosition);
    }
}
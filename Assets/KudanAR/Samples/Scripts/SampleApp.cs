using UnityEngine;
using System.Collections;

namespace Kudan.AR.Samples
{
	public class SampleApp : MonoBehaviour
	{
        public KudanTracker _kudanTracker;
        public TrackingMethodMarker _markerTracking;
        public TrackingMethodMarkerless _markerlessTracking;

        public float distanceMax;
        public float distanceMin;

        public void Start()
        {
            _kudanTracker.ChangeTrackingMethod(_markerlessTracking);

        }
    
        public void MarkerClicked()
        {
            _kudanTracker.ChangeTrackingMethod(_markerTracking);
        }

        public void MarkerlessClicked()
        {
            _kudanTracker.ChangeTrackingMethod(_markerlessTracking);
        }

        public void StartClicked()
        {
            // from the floor placer.
            Vector3 floorPosition;
            Quaternion floorOrientation;

            _kudanTracker.FloorPlaceGetPose(out floorPosition, out floorOrientation);
            _kudanTracker.ArbiTrackStart(floorPosition, floorOrientation);
        }

        public void StartMarkerless()
        {

            // from the floor placer.
            Vector3 floorPosition;
            Quaternion floorOrientation;
            _kudanTracker.ChangeTrackingMethod(_markerlessTracking);
            _kudanTracker.FloorPlaceGetPose(out floorPosition, out floorOrientation);
            _kudanTracker.ArbiTrackStart(floorPosition, floorOrientation);


        }

        public void spawnZombie()
        {
            Vector3 CameraPos = Camera.main.gameObject.transform.position;
            Vector3 floorPosition;
            Quaternion floorOrientation;
            //_kudanTracker.ChangeTrackingMethod(_markerlessTracking);
            _kudanTracker.FloorPlaceGetPose(out floorPosition, out floorOrientation);
            float ActualDist = Vector3.Distance(floorPosition, CameraPos);
            int randomDir = Random.Range(1,2);
            Vector3 offset = new Vector3(0, 0, 0);
            if (randomDir == 1)
                offset = new Vector3(0, 0 ,0);
            if (randomDir == 2)
                offset = new Vector3(0 , 0, 0);
            
            if (ActualDist > distanceMax)
            {
                Vector3 diffVec = floorPosition - CameraPos;
                diffVec.Normalize();
                float difference = ActualDist - distanceMax;
                
                diffVec.Scale(new Vector3(difference, 0, 0));
                _kudanTracker.ArbiTrackStart(floorPosition - (diffVec) + offset, floorOrientation);
            }   
            else if (ActualDist < distanceMin)
            {
                Vector3 diffVec = floorPosition - CameraPos;
                diffVec.Normalize();
                float difference = distanceMin - ActualDist;
                diffVec.Scale(new Vector3(difference * 2, 0, 0));
                _kudanTracker.ArbiTrackStart(floorPosition + (diffVec) + offset, floorOrientation);
            }
            else
              _kudanTracker.ArbiTrackStart(floorPosition, floorOrientation);
        } 

        void Update()
        {
            if (shootActions.allDead == true)
            {
                spawnZombie();
                shootActions.allDead = false;
            }

        }

    }


}
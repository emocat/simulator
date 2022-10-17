using UnityEngine;

namespace Simulator.Bridge.Data
{
    public class Trajectory
    {
        public double probability;
        public Apollo.TrajectoryPoint[] trajectory_point;
    }

    public class DetectedPredictionObject
    {
        public Detected3DObject Data;
        public Trajectory[] trajectory;
        public string Intent;
        public string Priority;
    }

    public class DetectedPredictionObjectData
    {
        public string Name;
        public string Frame;
        public double Time;
        public uint Sequence;
        public DetectedPredictionObject[] Data;
    }

    public class DetectedPredictionObjectArray
    {
        public DetectedPredictionObject[] Data;
    }
}

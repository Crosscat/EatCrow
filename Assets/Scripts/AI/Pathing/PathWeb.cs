using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using System;
using System.Linq;

public class PathWeb : MonoBehaviour
{
    public List<PathSegment> segments;

    private List<Vector2> _interpolatedPoints;
    public List<Vector2> InterpolatedPoints
    {
        get
        {
            if (_interpolatedPoints == null)
            {
                _interpolatedPoints = segments.SelectMany(it => it.InterpolationPoints).ToList();
            }

            return _interpolatedPoints;
        }
    }

    private Dictionary<PathNode, List<PathNode>> nodeNeighbors;

    private void Awake()
    {
        nodeNeighbors = new Dictionary<PathNode, List<PathNode>>();
        segments.ForEach(segment =>
        {
            if (!nodeNeighbors.ContainsKey(segment.From)) 
            {
                nodeNeighbors[segment.From] = new List<PathNode>();
            }
            if (!nodeNeighbors.ContainsKey(segment.To))
            {
                nodeNeighbors[segment.To] = new List<PathNode>();
            }

            nodeNeighbors[segment.From].Add(segment.To);
            if (!segment.OneWay)
                nodeNeighbors[segment.To].Add(segment.From);
        });
    }

    public List<Vector2> PathTo(Vector2 from, Vector2 to)
    {
        PathSegment startSegment = ClosestSegment(from);
        PathSegment endSegment = ClosestSegment(to);

        if (startSegment == endSegment
            || startSegment == null
            || endSegment == null)
        {
            return new List<Vector2>() { to };
        }
        //Simple brute force search
        Dictionary<PathNode, NodeDetail> nodeDetails = new Dictionary<PathNode, NodeDetail>();

        List<NodeDetail> nodesToExplore = new List<NodeDetail>();

        nodesToExplore.Add(new NodeDetail() { 
            node = startSegment.To,
            g = (startSegment.To.transform.position2() - from).magnitude
        });

        if (!startSegment.OneWay)
        {
            nodesToExplore.Add(new NodeDetail()
            {
                node = startSegment.From,
                g = (startSegment.From.transform.position2() - from).magnitude
            });
        }


        bool found = false;
        NodeDetail endNodeDetail = null;
        while (!found && nodesToExplore.Any())
        {
            nodesToExplore = nodesToExplore.OrderBy(it =>
                it.g + (endSegment.From.transform.position - it.node.transform.position).magnitude
            ).ToList();

            NodeDetail current = nodesToExplore[0];
            nodesToExplore.RemoveAt(0);

            foreach (PathNode next in nodeNeighbors[current.node])
            {
                float delta = (next.transform.position - current.node.transform.position).magnitude;
                float newG = current.g + delta;

                if (nodeDetails.ContainsKey(next))
                {
                    NodeDetail nextDetail = nodeDetails[next];
                    if (newG < nextDetail.g)
                    {
                        nextDetail.g = newG;
                        nextDetail.parent = current;
                    }
                }
                else
                {
                    NodeDetail nextDetail = new NodeDetail()
                    {
                        node = next,
                        parent = current,
                        g = newG
                    };

                    if (next == endSegment.From)
                    {
                        found = true;
                        endNodeDetail = nextDetail;
                        break;
                    }
                    if (!endSegment.OneWay && next == endSegment.To)
                    {
                        found = true;
                        endNodeDetail = nextDetail;
                        break;
                    }

                    nodeDetails[next] = nextDetail;
                    nodesToExplore.Add(nextDetail);
                }
            }
        }

        if (endNodeDetail == null) return null;

        List<Vector2> path = new List<Vector2>() { endNodeDetail.node.transform.position, to };
        NodeDetail previous = endNodeDetail.parent;
        while (previous != null)
        {
            path.Insert(0, previous.node.transform.position);
            previous = previous.parent;
        }

        return path;
    }

    private PathSegment ClosestSegment(Vector2 target)
    {
        PathSegment closestSegment = null;
        float closestDistance = float.MaxValue;

        PathSegment semiClosestSegment = null;
        float semiClosestDistance = float.MaxValue;


        foreach (PathSegment segment in segments)
        {
            Vector2 aToTarget = (target - segment.From.transform.position2());
            Vector2 bToTarget = (target - segment.To.transform.position2());
            float aProjectionLen = (Vector2.Dot(aToTarget, segment.Delta)) / segment.Length;
            float bProjectionLen = (Vector2.Dot(bToTarget, -segment.Delta)) / segment.Length;

            if (aProjectionLen >= 0 && bProjectionLen >= 0)
            {
                Vector2 projectionA = segment.Delta.normalized * aProjectionLen;
                Vector2 projectionB = -segment.Delta.normalized * bProjectionLen;
                float distanceA = (aToTarget - projectionA).magnitude;
                float distanceB = (bToTarget - projectionB).magnitude;

                float distance = Mathf.Min(distanceA, distanceB);

                if (distance < closestDistance)
                {
                    closestDistance = distance;
                    closestSegment = segment;
                }
            }

            float semiDistance = Mathf.Min(aToTarget.magnitude, bToTarget.magnitude);
            if (semiDistance < semiClosestDistance)
            {
                semiClosestDistance = semiDistance;
                semiClosestSegment = segment;
            }
        }
        return closestSegment;
    }

    private class NodeDetail
    {
        public PathNode node;
        public NodeDetail parent;
        public float g;
    }



    private void OnDrawGizmosSelected()
    {
        if (segments == null) return;

        segments.ForEach(it => it.OnDrawGizmosSelected());
    }
}

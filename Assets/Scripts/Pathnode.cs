using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Pathnode : MonoBehaviour {

	public Pathnode[] linkedNodes;
	public bool activated = false;

	// stuff used for finding the path
	public Pathnode destinationNode;
	List<Pathnode> openNodes = new List<Pathnode>();
	List<Pathnode> closedNodes = new List<Pathnode>();
	List<float> fDistances = new List<float>();
	List<float> gDistances = new List<float>();
	List<Pathnode> previousNode = new List<Pathnode>();
	List<Pathnode> previousNodeClosed = new List<Pathnode>();
	int currentNodeIndex;
	Pathnode currentNode;
	float shortestKnownPath;
	Pathnode mostOptimalPrevious;


	// Draw lines in editor to show connections
	void OnDrawGizmosSelected() {
		foreach (Pathnode node in linkedNodes) {
			if (node != null) {
				Debug.DrawLine(transform.position - Vector3.up, node.transform.position - Vector3.up, Color.yellow);    // Yellow indicates any connection
				Debug.DrawLine(transform.position, (transform.position + node.transform.position) / 2, Color.blue);     // So does blue, but it helps identify one way connections
			}
			else {
				print("Missing pathnode");
			}
		}
	}

	// If this node gets activated, it will draw a red line from this node to its destination. 
	// This script is completely useless, it will form just the foundation for actual scripts that involve moving.
	public void Update() {
        if (activated) {
			ResetPlot();
            activated = false;
            PlotMove(destinationNode);
        }
    }

	// Reset all the stuff so that it can find a path more than once. (In case the situation changes)
	public void ResetPlot() {
		openNodes.Clear();
		fDistances.Clear();
		gDistances.Clear();
		previousNode.Clear();
		closedNodes.Clear();
		previousNodeClosed.Clear();
	}

    public void PlotMove(Pathnode dest) {
        shortestKnownPath = Mathf.Infinity;

        //add the start node to the open Nodes set
        openNodes.Add(this);
        fDistances.Add(Vector3.Distance(transform.position, dest.transform.position));
        gDistances.Add(0);
        previousNode.Add(null);


        while (openNodes.Count > 0) {
            //figure out the node in the open set with the lowest f distance
            float fMin = Mathf.Infinity;
            foreach (float f in fDistances) {
                if (f < fMin) {
                    fMin = f;
                }
            }

            //select this node
            currentNodeIndex = fDistances.IndexOf(fMin);
            currentNode = openNodes[currentNodeIndex];


            //check if this node is the final destination
            if (currentNode == dest) {
                print("A path to the destination has been found with distance " + gDistances[currentNodeIndex]);
                if (gDistances[currentNodeIndex] < shortestKnownPath) {
                    shortestKnownPath = gDistances[currentNodeIndex];
                    mostOptimalPrevious = previousNode[currentNodeIndex];
                }
            }

            //if not, process this node, check its neighbors and put them to the open node list
            if (fDistances[currentNodeIndex] < shortestKnownPath) { // Don't bother checking if the current node is already further than the most optimal known path
                foreach (Pathnode node in currentNode.linkedNodes) {
                    if (!closedNodes.Contains(node)) {  // Check if the node hasn't already been processed.
                        float gDist = Vector3.Distance(currentNode.transform.position, node.transform.position) + gDistances[currentNodeIndex];
                        float hDist = Vector3.Distance(node.transform.position, dest.transform.position);
                        float fDist = gDist + hDist;
                        int nodeIndex = openNodes.IndexOf(node);
                        if (!openNodes.Contains(node)) {        // This node does not exist in the open note set yet
                            openNodes.Add(node);
                            fDistances.Add(fDist);
                            gDistances.Add(gDist);
                            previousNode.Add(currentNode);
                        }
                        else if (gDist < gDistances[nodeIndex]) {   // This node exists, but the route that's just been discovered is faster
                            fDistances[nodeIndex] = fDist;
                            gDistances[nodeIndex] = gDist;
                            previousNode[nodeIndex] = currentNode;
                        }
                    }
                }
            }

            //current node has been processed. so close it.
            closedNodes.Add(currentNode);
            previousNodeClosed.Add(previousNode[currentNodeIndex]);

            openNodes.RemoveAt(currentNodeIndex);
            fDistances.RemoveAt(currentNodeIndex);
            gDistances.RemoveAt(currentNodeIndex);
            previousNode.RemoveAt(currentNodeIndex);

        }

        if (shortestKnownPath < Mathf.Infinity) {
            // Solution found. 
            currentNode = dest;
            while (currentNode != this) {
                currentNodeIndex = closedNodes.IndexOf(currentNode);
                Debug.DrawLine(currentNode.transform.position, previousNodeClosed[currentNodeIndex].transform.position, Color.red, 20000);
                currentNode = previousNodeClosed[currentNodeIndex];
            }
            print("Destination reached. Shortest path: " + shortestKnownPath);
            return;
        }
        // No solution exists?
        print("A path could not be found. Does it not exist?");
    }
}

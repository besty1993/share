
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Assets.Scripts.MctsAi
{
    class Node
    {
        static Random r = new Random();
        static int nActions = 100;
        static double epsilon = 1e-6;

        public Node _parent;
        public string XmlFileName = "";
        //Node[] children;
        public List<Node> children;
        public double nVisits, totValue;
        public int depth;
        public int maxDepth = 5;

        public double x;
        public double y;

        public double p = 0;
        public double q = 0;

        public bool have_p = false;
        public List<double> ps;

        public Node(Node parent = null)
        {
            _parent = parent;
            
            nVisits = 0;
            totValue = 0;
            if (parent == null)
            {
                depth = 0;
                x = 0 - r.NextDouble();
                y = 0 - r.NextDouble();
            }
            else
            {
                depth = parent.depth + 1;
                x = 0-r.NextDouble();
                y = 0-r.NextDouble();
            }
            
        }

        public void ResetVisit()
        {
            nVisits = 0;
            if(children!=null)
            {
                foreach (Node node in children)
                {
                    node.ResetVisit();
                }
            }
        }

        public List<Node> SelectAction()
        {
            List<Node> visited = new List<Node>();
            Node cur = this;
            //visited.Add(this);
            while (!cur.IsLeaf())
            {
                cur = cur.Select();
                visited.Add(cur);
            }
            if(cur.nVisits >= 3 && cur.children == null)
            {
                if(cur.depth < maxDepth)
                {
                    cur.Expand();

                    

                    Node newNode = cur.Select();
                    visited.Add(newNode);
                }
            }
            //double value = rollOut(newNode);

            foreach (Node node in visited)
            {
                // would need extra logic for n-player game
                //node.updateStats(value);
            }

            return visited;
        }

        public void SetP(List<double> p)
        {
            ps = new List<double>();
            for(int i=0; i<p.Count; i++)
            {
                ps.Add(p[i]);
            }
            have_p = true;
        }

        public void Expand()
        {
            //children = new Node[nActions];
            //for (int i = 0; i < nActions; i++)
            //{
            //    children[i] = new Node(this);
            //}
            if (children != null) return;
            children = GetAllActions();
            //children = children.OrderBy(a => Guid.NewGuid()).ToList();
        }

        private Node Select()
        {
            Node selected = null;
            double bestValue = Double.MinValue;
            double bestq = Double.MinValue;
            Random random = new Random();
            foreach (Node c in children)
            {
                //Math.Sqrt
                //double uctValue = c.totValue / (c.nVisits + epsilon) +
                //Math.Sqrt(Math.Log(nVisits + 1) / (c.nVisits + epsilon)) +
                //r.NextDouble() * epsilon;
                double randomDouble = random.NextDouble();
                //c.p
                double uctValue = c.q + c.p / (c.nVisits + 1) *0.8 + randomDouble*0.2;
                //uctValue = c.q + 1 * Math.Sqrt(Math.Log(nVisits + 1) / (c.nVisits + 1));
                //uctValue = uctValue - c.nVisits * 200;
                //uctValue -= randomDouble*400;
                // small random number to break ties randomly in unexpanded nodes
                if (uctValue > bestValue)
                {
                    selected = c;
                    bestValue = uctValue;
                    bestq = c.q;
                }
                //uctValue += randomDouble*400;
             }
            //UnityEngine.Debug.Log(depth + ": " + bestq);
            //UnityEngine.Debug.Log("-----------------------------------------------------------");
            return selected;
        }

        public bool IsLeaf()
        {
            return children == null;
        }

        public double RollOut(Node tn)
        {
            // ultimately a roll out will end in some value
            // assume for now that it ends in a win or a loss
            // and just return this at random
            return r.Next(2);
        }

        public void UpdateStats(double value)
        {
            nVisits++;
            totValue += value;
            q = totValue / nVisits;
            //totValue -= 100;
            if (_parent != null) _parent.UpdateStats(value);

            if (value < 0.03) p = -1; 
        }

        public int Arity()
        {
            return  children.Count;
        }

        public Node GetMostVisted()
        {
            Node best = new Node(null) ;
            double bestvalue = double.MinValue;
            foreach (Node node in children)
            {
                if (node.totValue > bestvalue)
                {
                    bestvalue = node.totValue;
                    best = node;
                }
            }
            return best;
        }

        private List<Node> GetAllActions()
        {
            List<Node> allActions = new List<Node>();
            //List<Node> allNewActions = new List<Node>();
            int count = 0;
            for (int i = 0; i <= 10; i++)
            {
                for (int j = 0; j <= 20; j++)
                {
                    double x = ((double)i) / 10 - 1.0;
                    double y = ((double)j) / 10 - 1.0;

                    if (x * x + y * y <= 1 && x != 0)
                    {
                        Node node = new Node(this);
                        node.x = x;
                        node.y = y;
                        node.maxDepth = this.maxDepth;

                        if (!have_p)
                        {
                            if (y <= 0)
                            {
                                node.p = x * x + y * y;
                            }
                            else if (y > 0)
                            {
                                node.p = x * x - y * y;
                            }
                            else
                            {
                                node.p = 0;
                            }
                        }
                        else node.p = ps[count];

                        ++count;

                        allActions.Add(node);
                    }

                }
            }
            //for (int i = 0; i < allActions.Count; i++)
            //{
            //    if (allActions[i].x <= 0 && allActions[i].y <= 0)
            //    {
            //        allNewActions.Add(allActions[i]);
            //    }
            //}
            return allActions;
        }
    }
}

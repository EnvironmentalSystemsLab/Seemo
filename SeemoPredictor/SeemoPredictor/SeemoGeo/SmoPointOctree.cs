using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace SeemoPredictor.SeemoGeo
{
    /// <summary>
    /// A Dynamic Octree for storing any objects that can be described as a single point
    /// </summary>
    /// <remarks>
    /// Octree:	An octree is a tree data structure which divides 3D space into smaller partitions (nodes) 
    /// and places objects into the appropriate nodes. This allows fast access to objects
    /// in an area of interest without having to check every object.
    /// 
    /// Dynamic: The octree grows or shrinks as required when objects as added or removed.
    /// It also splits and merges nodes as appropriate. There is no maximum depth.
    /// Nodes have a constant - <see cref="PointOctree{T}.Node.NumObjectsAllowed"/> - which sets the amount of items allowed in a node before it splits.
    /// 
    /// </remarks>
    /// <typeparam name="T">The content of the octree can be anything, since the bounds data is supplied separately.</typeparam>
    public class SmoPointOctree<T>
    {
        /// <summary>
        /// Root node of the octree
        /// </summary>
        private SmoNode _rootNode;

        /// <summary>
        /// Size that the octree was on creation
        /// </summary>
        private readonly float _initialSize;

        /// <summary>
        /// Minimum side length that a node can be - essentially an alternative to having a max depth
        /// </summary>
        private readonly float _minSize;

        /// <summary>
        /// The total amount of objects currently in the tree
        /// </summary>
        public int Count { get; private set; }

        /// <summary>
        /// Gets the bounding box that represents the whole octree
        /// </summary>
        /// <value>The bounding box of the root node.</value>
        public SmoBBox MaxBounds
        {
            get { return new SmoBBox(_rootNode.Center, new SmoPoint3(_rootNode.SideLength, _rootNode.SideLength, _rootNode.SideLength)); }
        }

        /// <summary>
        /// Constructor for the point octree.
        /// </summary>
        /// <param name="initialWorldSize">Size of the sides of the initial node. The octree will never shrink smaller than this.</param>
        /// <param name="initialWorldPos">Position of the centre of the initial node.</param>
        /// <param name="minNodeSize">Nodes will stop splitting if the new nodes would be smaller than this.</param>
        public SmoPointOctree(float initialWorldSize, SmoPoint3 initialWorldPos, float minNodeSize)
        {
            if (minNodeSize > initialWorldSize)
            {
                Debug.WriteLine(
                    "Minimum node size must be at least as big as the initial world size. Was: " + minNodeSize
                    + " Adjusted to: " + initialWorldSize);
                minNodeSize = initialWorldSize;
            }
            Count = 0;
            _initialSize = initialWorldSize;
            _minSize = minNodeSize;
            _rootNode = new SmoNode(_initialSize, _minSize, initialWorldPos);
        }

        // #### PUBLIC METHODS ####

        /// <summary>
        /// Add an object.
        /// </summary>
        /// <param name="obj">Object to add.</param>
        /// <param name="objPos">Position of the object.</param>
        public void Add(T obj, SmoPoint3 objPos)
        {
            // Add object or expand the octree until it can be added
            int count = 0; //Safety check against infinite/excessive growth
            while (!_rootNode.Add(obj, objPos))
            {
                Grow(objPos - _rootNode.Center);
                if ( ++count > 20)
                {
                    Debug.WriteLine(
                        "Aborted Add operation as it seemed to be going on forever (" + (count - 1)
                        + ") attempts at growing the octree.");
                    return;
                }
            }
            Count++;
        }

        /// <summary>
        /// Remove an object. Makes the assumption that the object only exists once in the tree.
        /// </summary>
        /// <param name="obj">Object to remove.</param>
        /// <returns>True if the object was removed successfully.</returns>
        public bool Remove(T obj)
        {
            bool removed = _rootNode.Remove(obj);

            //See if we can shrink the octree down now that we've removed the item
            if (removed)
            {
                Count--;
                Shrink();
            }

            return removed;
        }

        /// <summary>
        /// Removes the specified object at the given position. Makes the assumption that the object only exists once in the tree.
        /// </summary>
        /// <param name="obj">Object to remove.</param>
        /// <param name="objPos">Position of the object.</param>
        /// <returns>True if the object was removed successfully.</returns>
        public bool Remove(T obj, SmoPoint3 objPos)
        {
            bool removed = _rootNode.Remove(obj, objPos);

            // See if we can shrink the octree down now that we've removed the item
            if (removed)
            {
                Count--;
                Shrink();
            }

            return removed;
        }

        /// <summary>
        /// Returns objects that are within <paramref name="maxDistance"/> of the specified ray.
        /// If none, returns an empty array (not null).
        /// </summary>
        /// <param name="ray">The ray. Passing as ref to improve performance since it won't have to be copied.</param>
        /// <param name="maxDistance">Maximum distance from the ray to consider.</param>
        /// <returns>Objects within range.</returns>
        public T[] GetNearby(SmoRay ray, float maxDistance)
        {
            List<T> collidingWith = new List<T>();
            _rootNode.GetNearby(ref ray, maxDistance, collidingWith);
            return collidingWith.ToArray();
        }

        /// <summary>
        /// Returns objects that are within <paramref name="maxDistance"/> of the specified position.
        /// If none, returns an empty array (not null).
        /// </summary>
        /// <param name="position">The position. Passing as ref to improve performance since it won't have to be copied.</param>
        /// <param name="maxDistance">Maximum distance from the position to consider.</param>
        /// <returns>Objects within range.</returns>
        public T[] GetNearby(SmoPoint3 position, float maxDistance)
        {
            List<T> collidingWith = new List<T>();
            _rootNode.GetNearby(ref position, maxDistance, collidingWith);
            return collidingWith.ToArray();
        }

        /// <summary>
        /// Returns all objects in the tree.
        /// If none, returns an empty array (not null).
        /// </summary>
        /// <returns>All objects.</returns>
        public ICollection<T> GetAll()
        {
            List<T> objects = new List<T>(Count);
            _rootNode.GetAll(objects);
            return objects;
        }

        // #### PRIVATE METHODS ####

        /// <summary>
        /// Grow the octree to fit in all objects.
        /// </summary>
        /// <param name="direction">Direction to grow.</param>
        private void Grow(SmoPoint3 direction)
        {
            int xDirection = direction.X >= 0 ? 1 : -1;
            int yDirection = direction.Y >= 0 ? 1 : -1;
            int zDirection = direction.Z >= 0 ? 1 : -1;
            SmoNode oldRoot = _rootNode;
            float half = _rootNode.SideLength / 2;
            float newLength = _rootNode.SideLength * 2;
            SmoPoint3 newCenter = _rootNode.Center + new SmoPoint3(xDirection * half, yDirection * half, zDirection * half);

            //Create  a new, bigger octree root node
            _rootNode = new SmoNode(newLength, _minSize, newCenter);

            if (oldRoot.HasAnyObjects())
            {
                //Create 7 new octree children to go wiht the old root as children of the new root
                int rootPos = _rootNode.BestFitChild(oldRoot.Center);
                SmoNode[] children = new SmoNode[8];
                for(int i = 0; i < 8; i++)
                {
                    if ( i == rootPos)
                    {
                        children[i] = oldRoot;
                    }
                    else
                    {
                        xDirection = i % 2 == 0 ? -1 : 1;
                        yDirection = i > 3 ? -1 : 1;
                        zDirection = (i < 2 || (i > 3 && i < 6)) ? -1 : 1;
                        children[i] = new SmoNode(
                            oldRoot.SideLength,
                            _minSize,
                            newCenter + new SmoPoint3(xDirection * half, yDirection * half, zDirection * half));
                    }
                }

                //Attach the new children to the new root node
                _rootNode.SetChildren(children);
            }
        }

        /// <summary>
        /// Shrink the octree if possible, else leave it the same.
        /// </summary>
        private void Shrink()
        {
            _rootNode = _rootNode.ShrinkIfPossible(_initialSize);
        }




        private class SmoNode
        {
            /// <summary>
            /// Center of this node
            /// </summary>
            public SmoPoint3 Center { get; private set; }

            /// <summary>
            /// Length of the sides of this node
            /// </summary>
            public float SideLength { get; private set; }

            /// <summary>
            /// Minimum size for a node in this octree
            /// </summary>
            private float _minSize;

            /// <summary>
            /// Bounding box that represents this node
            /// </summary>
            private SmoBBox _bounds = default(SmoBBox);

            /// <summary>
            /// Objects in this node
            /// </summary>
            private readonly List<SmoOctreeObject> _objects = new List<SmoOctreeObject>();

            /// <summary>
            /// Child nodes, if any
            /// </summary>
            private SmoNode[] _children = null;

            /// <summary>
            /// Bounds of potential children to this node. These are actual size (with looseness taken into account), not base size
            /// </summary>
            private SmoBBox[] _childBounds;

            /// <summary>
            /// If there are already NumObjectsAllowed in a node, we split it into children
            /// </summary>
            /// <remarks>
            /// A generally good number seems to be something around 8-15
            /// </remarks>
            private const int NumObjectsAllowed = 8;

            /// <summary>
            /// For reverting the bounds size after temporary changes
            /// </summary>
            private SmoPoint3 _actualBoundsSize;

            /// <summary>
            /// Gets a value indicating whether this node has children
            /// </summary>
            private bool HasChildren
            {
                get { return _children != null; }
            }

            /// <summary>
            /// An object in the octree
            /// </summary>
            private class SmoOctreeObject
            {
                /// <summary>
                /// Object content
                /// </summary>
                public T Obj;

                /// <summary>
                /// Object position
                /// </summary>
                public SmoPoint3 Pos;
            }

            /// <summary>
            /// Constructor.
            /// </summary>
            /// <param name="baseLengthVal">Length of this node, not taking looseness into account.</param>
            /// <param name="minSizeVal">Minimum size of nodes in this octree.</param>
            /// <param name="centerVal">Center position of this node.</param>
            public SmoNode(float baseLengthVal, float minSizeVal, SmoPoint3 centerVal)
            {
                SetValues(baseLengthVal, minSizeVal, centerVal);
            }

            // #### PUBLIC METHODS ####

            /// <summary>
            /// Add an object.
            /// </summary>
            /// <param name="obj">Object to add.</param>
            /// <param name="objPos">Position of the object.</param>
            /// <returns></returns>
            public bool Add(T obj, SmoPoint3 objPos)
            {
                if(!Encapsulates(_bounds, objPos))
                {
                    return false;
                }
                SubAdd(obj, objPos);
                return true;
            }

            /// <summary>
            /// Remove an object. Makes the assumption that the object only exists once in the tree.
            /// </summary>
            /// <param name="obj">Object to remove.</param>
            /// <returns>True if the object was removed successfully.</returns>
            public bool Remove(T obj)
            {
                bool removed = false;

                for(int i = 0; i < _objects.Count; i++)
                {
                    if (_objects[i].Obj.Equals(obj))
                    {
                        removed = _objects.Remove(_objects[i]);
                        break;
                    }
                }
                if (!removed && _children != null)
                {
                    for (int i = 0; i < 8; i++)
                    {
                        removed = _children[i].Remove(obj);
                        if (removed) break;
                    }
                }

                if (!removed && _children != null)
                {
                    //Check if we should merge nodes now that we've removed an item
                    if (ShouldMerge())
                    {
                        Merge();
                    }
                }

                return removed;
            }

            /// <summary>
            /// Removes the specified object at the given position. Makes the assumption that the object only exists once in the tree.
            /// </summary>
            /// <param name="obj">Object to remove.</param>
            /// <param name="objPos">Position of the object.</param>
            /// <returns>True if the object was removed successfully.</returns>

            public bool Remove(T obj, SmoPoint3 objPos)
            {
                if(!Encapsulates(_bounds, objPos))
                {
                    return false;
                }
                return SubRemove(obj, objPos);
            }

            /// <summary>
            /// Return objects that are within <paramref name="maxDistance"/> of the specified ray.
            /// </summary>
            /// <param name="ray">The ray.</param>
            /// <param name="maxDistance">Maximum distance from the ray to consider.</param>
            /// <param name="result">List result.</param>
            /// <returns>Objects within range.</returns>
            public void GetNearby(ref SmoRay ray, float maxDistance, List<T> result)
            {
                // Does the ray hit this node at all?
                // Note: Expanding the bounds is not exactly the same as a real distance check, but it's fast.
                // TODO: Does someone have a fast AND accurate formula to do this check?
                _bounds.Expand(new SmoPoint3(maxDistance * 2, maxDistance * 2, maxDistance * 2));
                bool intersected = _bounds.IntersectRay(ray);
                _bounds.Size = _actualBoundsSize;
                if (!intersected)
                {
                    return;
                }

                //Check against any objects in this node
                for (int i = 0; i < _objects.Count; i++)
                {
                    if (SqrDistanceToRay(ray, _objects[i].Pos) <= (maxDistance * maxDistance))
                    {
                        result.Add(_objects[i].Obj);
                    }
                }

                //Check children
                if (_children != null)
                {
                    for (int i = 0; i < 8; i++)
                    {
                        _children[i].GetNearby(ref ray, maxDistance, result);
                    }
                }
            }

            /// <summary>
            /// Return objects that are within <paramref name="maxDistance"/> of the specified position.
            /// </summary>
            /// <param name="position">The position.</param>
            /// <param name="maxDistance">Maximum distance from the position to consider.</param>
            /// <param name="result">List result.</param>
            /// <returns>Objects within range.</returns>
            public void GetNearby(ref SmoPoint3 position, float maxDistance, List<T> result)
            {
                // Does the node contain this position at all?
                // Note: Expanding the bounds is not exactly the same as a real distance check, but it's fast.
                // TODO: Does someone have a fast AND accurate formula to do this check?
                _bounds.Expand(new SmoPoint3(maxDistance * 2, maxDistance * 2, maxDistance * 2));
                bool contained = _bounds.Contains(position);
                _bounds.Size = _actualBoundsSize;
                if (!contained)
                {
                    return;
                }

                //Check against any objects in this node
                for( int i = 0; i < _objects.Count; i++)
                {
                    if(SmoPoint3.Distance(position, _objects[i].Pos) <= maxDistance)
                    {
                        result.Add(_objects[i].Obj);
                    }
                }

                //Check children
                if(_children != null)
                {
                    for(int i = 0; i<8; i++)
                    {
                        _children[i].GetNearby(ref position, maxDistance, result);
                    }
                }
            }

            /// <summary>
            /// Return all objects in the tree.
            /// </summary>
            /// <returns>All objects.</returns>
            public void GetAll(List<T> result)
            {
                //add directly contained objects
                result.AddRange(_objects.Select(o => o.Obj));

                //add children objects
                if(_children != null)
                {
                    for (int i = 0; i < 8; i++)
                    {
                        _children[i].GetAll(result);
                    }
                }
            }

            /// <summary>
            /// Set the 8 children of this octree.
            /// </summary>
            /// <param name="childOctrees">The 8 new child nodes.</param>
            public void SetChildren(SmoNode[] childOctrees)
            {
                if(childOctrees.Length != 8)
                {
                    Debug.WriteLine("Child octree array must be length 8. Was length: " + childOctrees.Length);
                    return;
                }

                _children = childOctrees;
            }

            /// <summary>
            /// We can shrink the octree if:
            /// - This node is >= double minLength in length
            /// - All objects in the root node are within one octant
            /// - This node doesn't have children, or does but 7/8 children are empty
            /// We can also shrink it if there are no objects left at all!
            /// </summary>
            /// <param name="minLength">Minimum dimensions of a node in this octree.</param>
            /// <returns>The new root, or the existing one if we didn't shrink.</returns>
            public SmoNode ShrinkIfPossible(float minLength)
            {
                if(SideLength < (2* minLength))
                {
                    return this;
                }
                if(_objects.Count == 0 && (_children == null || _children.Length == 0))
                {
                    return this;
                }

                //Check objects in root
                int bestFit = -1;
                for (int i = 0; i < _objects.Count; i++)
                {
                    SmoOctreeObject curObj = _objects[i];
                    int newBestFit = BestFitChild(curObj.Pos);
                    if (i == 0 || newBestFit == bestFit)
                    {
                        if (bestFit < 0)
                        {
                            bestFit = newBestFit;
                        }
                    }
                    else
                    {
                        return this; // Can't reduce - objects fit in different octants
                    }
                }

                //Check objects in children if there are any
                if (_children != null)
                {
                    bool childHadContent = false;
                    for (int i = 0; i < _children.Length; i++)
                    {
                        if (_children[i].HasAnyObjects())
                        {
                            if (childHadContent)
                            {
                                return this; // Can't shrink - another child had content already
                            }
                            if (bestFit >= 0 && bestFit != i)
                            {
                                return this; // Can't reduce - objects in root are in a different octant to objects in child
                            }
                            childHadContent = true;
                            bestFit = i;
                        }
                    }
                }

                //Can reduce
                if (_children == null)
                {
                    // We don't have any children, so just shrink this node to the new size
                    // We already know that everything will still fit in it
                    SetValues(SideLength / 2, _minSize, _childBounds[bestFit].Center);
                    return this;
                }

                // We have children. Use the appropriate child as the new root node
                return _children[bestFit];
            }

            /// <summary>
            /// Find which child node this object would be most likely to fit in.
            /// </summary>
            /// <param name="objPos">The object's position.</param>
            /// <returns>One of the eight child octants.</returns>
            public int BestFitChild(SmoPoint3 objPos)
            {
                return (objPos.X <= Center.X ? 0 : 1) + (objPos.Y >= Center.Y ? 0 : 4) + (objPos.Z <= Center.Z ? 0 : 2);
            }

            /// <summary>
            /// Checks if this node or anything below it has something in it.
            /// </summary>
            /// <returns>True if this node or any of its children, grandchildren etc have something in them</returns>
            public bool HasAnyObjects()
            {
                if (_objects.Count > 0) return true;
                
                if (_children != null)
                {
                    for (int i = 0; i < 8; i++)
                    {
                        if (_children[i].HasAnyObjects()) return true;
                    }
                }

                return false;
            }

            /// <summary>
            /// Returns the squared distance to the given ray from a point.
            /// </summary>
            /// <param name="ray">The ray.</param>
            /// <param name="point">The point to check distance from the ray.</param>
            /// <returns>Squared distance from the point to the closest point of the ray.</returns>
            public static float SqrDistanceToRay(SmoRay ray, SmoPoint3 point)
            {
                return SmoPoint3.Cross(ray.Direction, point - ray.Origin).SqrMagnitude;
            }

            //#### PRIVATE METHODS ####

            /// <summary>
            /// Set values for this node. 
            /// </summary>
            /// <param name="baseLengthVal">Length of this node, not taking looseness into account.</param>
            /// <param name="minSizeVal">Minimum size of nodes in this octree.</param>
            /// <param name="centerVal">Centre position of this node.</param>
            private void SetValues(float baseLengthVal, float minSizeVal, SmoPoint3 centerVal)
            {
                SideLength = baseLengthVal;
                _minSize = minSizeVal;
                Center = centerVal;

                //Create the bounding box.
                _actualBoundsSize = new SmoPoint3(SideLength, SideLength, SideLength);
                _bounds = new SmoBBox(Center, _actualBoundsSize);

                float quarter = SideLength / 4f;
                float childActualLength = SideLength / 2;
                SmoPoint3 childActualSize = new SmoPoint3(childActualLength, childActualLength, childActualLength);
                _childBounds = new SmoBBox[8];
                _childBounds[0] = new SmoBBox(Center + new SmoPoint3(-quarter, quarter, -quarter), childActualSize);
                _childBounds[1] = new SmoBBox(Center + new SmoPoint3(quarter, quarter, -quarter), childActualSize);
                _childBounds[2] = new SmoBBox(Center + new SmoPoint3(-quarter, quarter, quarter), childActualSize);
                _childBounds[3] = new SmoBBox(Center + new SmoPoint3(quarter, quarter, quarter), childActualSize);
                _childBounds[4] = new SmoBBox(Center + new SmoPoint3(-quarter, -quarter, -quarter), childActualSize);
                _childBounds[5] = new SmoBBox(Center + new SmoPoint3(quarter, -quarter, -quarter), childActualSize);
                _childBounds[6] = new SmoBBox(Center + new SmoPoint3(-quarter, -quarter, quarter), childActualSize);
                _childBounds[7] = new SmoBBox(Center + new SmoPoint3(quarter, -quarter, quarter), childActualSize);
            }

            /// <summary>
            /// Private counterpart to the public Add method.
            /// </summary>
            /// <param name="obj">Object to add.</param>
            /// <param name="objPos">Position of the object.</param>
            private void SubAdd(T obj, SmoPoint3 objPos)
            {
                // We know it fits at this level if we've got this far

                // We always put things in the deepest possible child
                // So we can skip checks and simply move down if there are children aleady
                if (!HasChildren)
                {
                    //Just add if few objects are here, or children would be below min size
                    if (_objects.Count < NumObjectsAllowed || (SideLength / 2) < _minSize)
                    {
                        SmoOctreeObject newObj = new SmoOctreeObject { Obj = obj, Pos = objPos };
                        _objects.Add(newObj);
                        return; // We're Done. No Children yet
                    }

                    //Enough objects in this node already : create the 8 children
                    int bestFitChild;
                    if (_children == null)
                    {
                        Split();
                        if(_children == null)
                        {
                            Debug.WriteLine("Child creation failed for an unkown reason. Early exit.");
                            return;
                        }

                        //Now that we have the new children, move this node's existing objects into them
                        for (int i = _objects.Count - 1; i >= 0; i--)
                        {
                            SmoOctreeObject existingObj = _objects[i];
                            //Find which child the object is closest to based on where the
                            //object's center is located in relation to the octree's center
                            bestFitChild = BestFitChild(existingObj.Pos);
                            _children[bestFitChild].SubAdd(existingObj.Obj, existingObj.Pos); // Go a level deeper
                            _objects.Remove(existingObj); // Remove from here
                        }
                    }
                }

                //Handle the new object we're adding now
                int bestFit = BestFitChild(objPos);
                _children[bestFit].SubAdd(obj, objPos);
            }

            /// <summary>
            /// Private counterpart to the public <see cref="Remove(T, Point3)"/> method.
            /// </summary>
            /// <param name="obj">Object to remove.</param>
            /// <param name="objPos">Position of the object.</param>
            /// <returns>True if the object was removed successfully.</returns>
            private bool SubRemove(T obj, SmoPoint3 objPos)
            {
                bool removed = false;
                
                for (int i =0; i < _objects.Count; i++)
                {
                    if (_objects[i].Obj.Equals(obj))
                    {
                        removed = _objects.Remove(_objects[i]);
                        break;
                    }
                }

                if (!removed && _children != null)
                {
                    int bestFitChild = BestFitChild(objPos);
                    removed = _children[bestFitChild].SubRemove(obj, objPos);
                }

                if(removed && _children != null)
                {
                    // Check if we should merge nodes now that we've removed an item
                    if (ShouldMerge())
                    {
                        Merge();
                    }
                }
                return removed;

            }

            /// <summary>
            /// Splits the octree into eight children.
            /// </summary>
            private void Split()
            {
                float quarter = SideLength / 4f;
                float newLength = SideLength / 2;
                _children = new SmoNode[8];
                _children[0] = new SmoNode(newLength, _minSize, Center + new SmoPoint3(-quarter, quarter, -quarter));
                _children[1] = new SmoNode(newLength, _minSize, Center + new SmoPoint3(quarter, quarter, -quarter));
                _children[2] = new SmoNode(newLength, _minSize, Center + new SmoPoint3(-quarter, quarter, quarter));
                _children[3] = new SmoNode(newLength, _minSize, Center + new SmoPoint3(quarter, quarter, quarter));
                _children[4] = new SmoNode(newLength, _minSize, Center + new SmoPoint3(-quarter, -quarter, -quarter));
                _children[5] = new SmoNode(newLength, _minSize, Center + new SmoPoint3(quarter, -quarter, -quarter));
                _children[6] = new SmoNode(newLength, _minSize, Center + new SmoPoint3(-quarter, -quarter, quarter));
                _children[7] = new SmoNode(newLength, _minSize, Center + new SmoPoint3(quarter, -quarter, quarter));
            }

            /// <summary>
            /// Merge all children into this node - the opposite of Split.
            /// Note: We only have to check one level down since a merge will never happen if the children already have children,
            /// since THAT won't happen unless there are already too many objects to merge.
            /// </summary>
            private void Merge()
            {
                //Note: We know children != null or we wouldn't be merging
                for(int i = 0; i < 8; i++)
                {
                    SmoNode curChild = _children[i];
                    int numObjects = curChild._objects.Count;
                    for ( int j = numObjects - 1; j >= 0; j--)
                    {
                        SmoOctreeObject curObj = curChild._objects[j];
                        _objects.Add(curObj);
                    }
                }
                // Remove the child nodes (and the objects in them - they've been added elsewhere now)
                _children = null;
            }

            /// <summary>
            /// Checks if outerBounds encapsulates the given point.
            /// </summary>
            /// <param name="outerBounds">Outer bounds.</param>
            /// <param name="point">Point.</param>
            /// <returns>True if innerBounds is fully encapsulated by outerBounds.</returns>
            private static bool Encapsulates(SmoBBox outerBounds, SmoPoint3 point)
            {
                return outerBounds.Contains(point);
            }

            /// <summary>
            /// Checks if there are few enough objects in this node and its children that the children should all be merged into this.
            /// </summary>
            /// <returns>True there are less or the same amount of objects in this and its children than <see cref="NumObjectsAllowed"/>.</returns>
            private bool ShouldMerge()
            {
                int totalObjects = _objects.Count;
                if (_children != null)
                {
                    foreach (SmoNode child in _children)
                    {
                        if (child._children != null)
                        {
                            // If any of the *children* have children, there are definitely too many to merge,
                            // or the child would have been merged already
                            return false;
                        }
                        totalObjects += child._objects.Count;
                    }
                }
                return totalObjects <= NumObjectsAllowed;
            }
        }
    }
}

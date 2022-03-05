using System.Collections.Generic;
using System.Diagnostics;

namespace SeemoPredictor.Geometry
{
    /// <summary>
    /// A Dynamic, Loose Octree for storing any objects that can be described with AABB bounds
    /// </summary>
    /// <seealso cref="BoundsOctree{T}"/>
    /// <remarks>
    /// Octree:	An octree is a tree data structure which divides 3D space into smaller partitions (nodes)
    /// and places objects into the appropriate nodes. This allows fast access to objects
    /// in an area of interest without having to check every object.
    /// 
    /// Dynamic: The octree grows or shrinks as required when objects as added or removed.
    /// It also splits and merges nodes as appropriate. There is no maximum depth.
    /// Nodes have a constant - <see cref="BoundsOctree{T}.BoundsOctreeNode.NumObjectsAllowed"/> - which sets the amount of items allowed in a node before it splits.
    /// 
    /// Loose: The octree's nodes can be larger than 1/2 their parent's length and width, so they overlap to some extent.
    /// This can alleviate the problem of even tiny objects ending up in large nodes if they're near boundaries.
    /// A looseness value of 1.0 will make it a "normal" octree.
    /// 
    /// Note: For loops are often used here since in some cases (e.g. the IsColliding method)
    /// they actually give much better performance than using Foreach, even in the compiled build.
    /// Using a LINQ expression is worse again than Foreach.
    /// 
    /// See also: PointOctree, where objects are stored as single points and some code can be simplified
    /// </remarks>
    /// <typeparam name="T">The content of the octree can be anything, since the bounds data is supplied separately.</typeparam>
    public  class BoundsOctree<T>
    {
   

        /// <summary>
        /// Root node of the octree
        /// </summary>
        private BoundsOctreeNode _rootNode;

        /// <summary>
        /// Should be a value between 1 and 2. A multiplier for the base size of a node.
        /// </summary>
        /// <remarks>
        /// 1.0 is a "normal" octree, while values > 1 have overlap
        /// </remarks>
        private readonly float _looseness;

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
		public BBox MaxBounds
        {
            get { return _rootNode.Bounds; }
        }

        /// <summary>
        /// Constructor for the bounds octree.
        /// </summary>
        /// <param name="initialWorldSize">Size of the sides of the initial node, in metres. The octree will never shrink smaller than this.</param>
        /// <param name="initialWorldPos">Position of the center of the initial node.</param>
        /// <param name="minNodeSize">Nodes will stop splitting if the new nodes would be smaller than this (metres).</param>
        /// <param name="loosenessVal">Clamped between 1 and 2. Values > 1 let nodes overlap.</param>
        public BoundsOctree(float initialWorldSize, Point3 initialWorldPos, float minNodeSize, float loosenessVal)
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
            _looseness = MathExtensions.Clamp(loosenessVal, 1.0f, 2.0f);
            _rootNode = new BoundsOctreeNode(_initialSize, _minSize, _looseness, initialWorldPos);
        }

        // #### PUBLIC METHODS ####

        /// <summary>
        /// Add an object.
        /// </summary>
        /// <param name="obj">Object to add.</param>
        /// <param name="objBounds">3D bounding box around the object.</param>
        public void Add(T obj, BBox objBounds)
        {
            // Add object or expand the octree until it can be added
            int count = 0; // Safety check against infinite/excessive growth
            while (!_rootNode.Add(obj, objBounds))
            {
                Grow(objBounds.Center - _rootNode.Center);
                if (++count > 20)
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

            // See if we can shrink the octree down now that we've removed the item
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
        /// <param name="objBounds">3D bounding box around the object.</param>
        /// <returns>True if the object was removed successfully.</returns>
        public bool Remove(T obj, BBox objBounds)
        {
            bool removed = _rootNode.Remove(obj, objBounds);

            // See if we can shrink the octree down now that we've removed the item
            if (removed)
            {
                Count--;
                Shrink();
            }

            return removed;
        }

        /// <summary>
        /// Check if the specified bounds intersect with anything in the tree. See also: GetColliding.
        /// </summary>
        /// <param name="checkBounds">bounds to check.</param>
        /// <returns>True if there was a collision.</returns>
        public bool IsColliding(BBox checkBounds)
        {
            return _rootNode.IsColliding(ref checkBounds);
        }

        /// <summary>
        /// Check if the specified ray intersects with anything in the tree. See also: GetColliding.
        /// </summary>
        /// <param name="checkRay">ray to check.</param>
        /// <param name="maxDistance">distance to check.</param>
        /// <returns>True if there was a collision.</returns>
        public bool IsColliding(Ray checkRay, float maxDistance)
        {
            return _rootNode.IsColliding(ref checkRay, maxDistance);
        }

        /// <summary>
        /// Returns an array of objects that intersect with the specified bounds, if any. Otherwise returns an empty array. See also: IsColliding.
        /// </summary>
        /// <param name="collidingWith">list to store intersections.</param>
        /// <param name="checkBounds">bounds to check.</param>
        /// <returns>Objects that intersect with the specified bounds.</returns>
        public void GetColliding(List<T> collidingWith, BBox checkBounds)
        {
            _rootNode.GetColliding(ref checkBounds, collidingWith);
        }

        /// <summary>
        /// Returns an array of objects that intersect with the specified ray, if any. Otherwise returns an empty array. See also: IsColliding.
        /// </summary>
        /// <param name="collidingWith">list to store intersections.</param>
        /// <param name="checkRay">ray to check.</param>
        /// <param name="maxDistance">distance to check.</param>
        /// <returns>Objects that intersect with the specified ray.</returns>
        public void GetColliding(List<T> collidingWith, Ray checkRay, float maxDistance = float.PositiveInfinity)
        {
            _rootNode.GetColliding(ref checkRay, collidingWith, maxDistance);
        }

        // #### PRIVATE METHODS ####

        /// <summary>
        /// Grow the octree to fit in all objects.
        /// </summary>
        /// <param name="direction">Direction to grow.</param>
        private void Grow(Point3 direction)
        {
            int xDirection = direction.X >= 0 ? 1 : -1;
            int yDirection = direction.Y >= 0 ? 1 : -1;
            int zDirection = direction.Z >= 0 ? 1 : -1;
            BoundsOctreeNode oldRoot = _rootNode;
            float half = _rootNode.BaseLength / 2;
            float newLength = _rootNode.BaseLength * 2;
            Point3 newCenter = _rootNode.Center + new Point3(xDirection * half, yDirection * half, zDirection * half);

            // Create a new, bigger octree root node
            _rootNode = new BoundsOctreeNode(newLength, _minSize, _looseness, newCenter);

            if (oldRoot.HasAnyObjects())
            {
                // Create 7 new octree children to go with the old root as children of the new root
                int rootPos = _rootNode.BestFitChild(oldRoot.Center);
                BoundsOctreeNode[] children = new BoundsOctreeNode[8];
                for (int i = 0; i < 8; i++)
                {
                    if (i == rootPos)
                    {
                        children[i] = oldRoot;
                    }
                    else
                    {
                        xDirection = i % 2 == 0 ? -1 : 1;
                        yDirection = i > 3 ? -1 : 1;
                        zDirection = (i < 2 || (i > 3 && i < 6)) ? -1 : 1;
                        children[i] = new BoundsOctreeNode(
                            oldRoot.BaseLength,
                            _minSize,
                            _looseness,
                            newCenter + new Point3(xDirection * half, yDirection * half, zDirection * half));
                    }
                }

                // Attach the new children to the new root node
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






        /// <summary>
        /// A node in a BoundsOctree
        /// </summary>
        private class BoundsOctreeNode
        {


            /// <summary>
            /// Centre of this node
            /// </summary>
            public Point3 Center { get; private set; }

            /// <summary>
            /// Length of this node if it has a looseness of 1.0
            /// </summary>
            public float BaseLength { get; private set; }

            /// <summary>
            /// Looseness value for this node
            /// </summary>
            private float _looseness;

            /// <summary>
            /// Minimum size for a node in this octree
            /// </summary>
            private float _minSize;

            /// <summary>
            /// Actual length of sides, taking the looseness value into account
            /// </summary>
            private float _adjLength;

            /// <summary>
            /// Bounding box that represents this node
            /// </summary>
            private BBox _bounds = default(BBox);

            /// <summary>
            /// Objects in this node
            /// </summary>
            private readonly List<OctreeObject> _objects = new List<OctreeObject>();

            /// <summary>
            /// Child nodes, if any
            /// </summary>
            private BoundsOctreeNode[] _children = null;

            /// <summary>
            /// Bounds of potential children to this node. These are actual size (with looseness taken into account), not base size
            /// </summary>
            private BBox[] _childBounds;

            /// <summary>
            /// If there are already NumObjectsAllowed in a node, we split it into children
            /// </summary>
            /// <remarks>
            /// A generally good number seems to be something around 8-15
            /// </remarks>
            private const int NumObjectsAllowed = 8;

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
            private class OctreeObject
            {
                /// <summary>
                /// Object content
                /// </summary>
                public T Obj;

                /// <summary>
                /// Object bounds
                /// </summary>
                public BBox Bounds;
            }

            /// <summary>
            /// Gets the bounding box that represents this node
            /// </summary>
            public BBox Bounds
            {
                get { return _bounds; }
            }

            /// <summary>
            /// Constructor.
            /// </summary>
            /// <param name="baseLengthVal">Length of this node, not taking looseness into account.</param>
            /// <param name="minSizeVal">Minimum size of nodes in this octree.</param>
            /// <param name="loosenessVal">Multiplier for baseLengthVal to get the actual size.</param>
            /// <param name="centerVal">Centre position of this node.</param>
            public BoundsOctreeNode(float baseLengthVal, float minSizeVal, float loosenessVal, Point3 centerVal)
            {
                SetValues(baseLengthVal, minSizeVal, loosenessVal, centerVal);
            }

            // #### PUBLIC METHODS ####

            /// <summary>
            /// Add an object.
            /// </summary>
            /// <param name="obj">Object to add.</param>
            /// <param name="objBounds">3D bounding box around the object.</param>
            /// <returns>True if the object fits entirely within this node.</returns>
            public bool Add(T obj, BBox objBounds)
            {
                if (!Encapsulates(_bounds, objBounds))
                {
                    return false;
                }
                SubAdd(obj, objBounds);
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

                for (int i = 0; i < _objects.Count; i++)
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

                if (removed && _children != null)
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
            /// Removes the specified object at the given position. Makes the assumption that the object only exists once in the tree.
            /// </summary>
            /// <param name="obj">Object to remove.</param>
            /// <param name="objBounds">3D bounding box around the object.</param>
            /// <returns>True if the object was removed successfully.</returns>
            public bool Remove(T obj, BBox objBounds)
            {
                if (!Encapsulates(_bounds, objBounds))
                {
                    return false;
                }
                return SubRemove(obj, objBounds);
            }

            /// <summary>
            /// Check if the specified bounds intersect with anything in the tree. See also: GetColliding.
            /// </summary>
            /// <param name="checkBounds">Bounds to check.</param>
            /// <returns>True if there was a collision.</returns>
            public bool IsColliding(ref BBox checkBounds)
            {
                // Are the input bounds at least partially in this node?
                if (!_bounds.Intersects(checkBounds))
                {
                    return false;
                }

                // Check against any objects in this node
                for (int i = 0; i < _objects.Count; i++)
                {
                    if (_objects[i].Bounds.Intersects(checkBounds))
                    {
                        return true;
                    }
                }

                // Check children
                if (_children != null)
                {
                    for (int i = 0; i < 8; i++)
                    {
                        if (_children[i].IsColliding(ref checkBounds))
                        {
                            return true;
                        }
                    }
                }

                return false;
            }

            /// <summary>
            /// Check if the specified ray intersects with anything in the tree. See also: GetColliding.
            /// </summary>
            /// <param name="checkRay">Ray to check.</param>
            /// <param name="maxDistance">Distance to check.</param>
            /// <returns>True if there was a collision.</returns>
            public bool IsColliding(ref Ray checkRay, float maxDistance = float.PositiveInfinity)
            {
                // Is the input ray at least partially in this node?
                float distance;
                if (!_bounds.IntersectRay(checkRay, out distance) || distance > maxDistance)
                {
                    return false;
                }

                // Check against any objects in this node
                for (int i = 0; i < _objects.Count; i++)
                {
                    if (_objects[i].Bounds.IntersectRay(checkRay, out distance) && distance <= maxDistance)
                    {
                        return true;
                    }
                }

                // Check children
                if (_children != null)
                {
                    for (int i = 0; i < 8; i++)
                    {
                        if (_children[i].IsColliding(ref checkRay, maxDistance))
                        {
                            return true;
                        }
                    }
                }

                return false;
            }

            /// <summary>
            /// Returns an array of objects that intersect with the specified bounds, if any. Otherwise returns an empty array. See also: IsColliding.
            /// </summary>
            /// <param name="checkBounds">Bounds to check. Passing by ref as it improves performance with structs.</param>
            /// <param name="result">List result.</param>
            /// <returns>Objects that intersect with the specified bounds.</returns>
            public void GetColliding(ref BBox checkBounds, List<T> result)
            {
                // Are the input bounds at least partially in this node?
                if (!_bounds.Intersects(checkBounds))
                {
                    return;
                }

                // Check against any objects in this node
                for (int i = 0; i < _objects.Count; i++)
                {
                    if (_objects[i].Bounds.Intersects(checkBounds))
                    {
                        result.Add(_objects[i].Obj);
                    }
                }

                // Check children
                if (_children != null)
                {
                    for (int i = 0; i < 8; i++)
                    {
                        _children[i].GetColliding(ref checkBounds, result);
                    }
                }
            }

            /// <summary>
            /// Returns an array of objects that intersect with the specified ray, if any. Otherwise returns an empty array. See also: IsColliding.
            /// </summary>
            /// <param name="checkRay">Ray to check. Passing by ref as it improves performance with structs.</param>
            /// <param name="maxDistance">Distance to check.</param>
            /// <param name="result">List result.</param>
            /// <returns>Objects that intersect with the specified ray.</returns>
            public void GetColliding(ref Ray checkRay, List<T> result, float maxDistance = float.PositiveInfinity)
            {
                float distance;
                // Is the input ray at least partially in this node?
                if (!_bounds.IntersectRay(checkRay, out distance) || distance > maxDistance)
                {
                    return;
                }

                // Check against any objects in this node
                for (int i = 0; i < _objects.Count; i++)
                {
                    if (_objects[i].Bounds.IntersectRay(checkRay, out distance) && distance <= maxDistance)
                    {
                        result.Add(_objects[i].Obj);
                    }
                }

                // Check children
                if (_children != null)
                {
                    for (int i = 0; i < 8; i++)
                    {
                        _children[i].GetColliding(ref checkRay, result, maxDistance);
                    }
                }
            }

            /// <summary>
            /// Set the 8 children of this octree.
            /// </summary>
            /// <param name="childOctrees">The 8 new child nodes.</param>
            public void SetChildren(BoundsOctreeNode[] childOctrees)
            {
                if (childOctrees.Length != 8)
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
            public BoundsOctreeNode ShrinkIfPossible(float minLength)
            {
                if (BaseLength < (2 * minLength))
                {
                    return this;
                }
                if (_objects.Count == 0 && (_children == null || _children.Length == 0))
                {
                    return this;
                }

                // Check objects in root
                int bestFit = -1;
                for (int i = 0; i < _objects.Count; i++)
                {
                    OctreeObject curObj = _objects[i];
                    int newBestFit = BestFitChild(curObj.Bounds.Center);
                    if (i == 0 || newBestFit == bestFit)
                    {
                        // In same octant as the other(s). Does it fit completely inside that octant?
                        if (Encapsulates(_childBounds[newBestFit], curObj.Bounds))
                        {
                            if (bestFit < 0)
                            {
                                bestFit = newBestFit;
                            }
                        }
                        else
                        {
                            // Nope, so we can't reduce. Otherwise we continue
                            return this;
                        }
                    }
                    else
                    {
                        return this; // Can't reduce - objects fit in different octants
                    }
                }

                // Check objects in children if there are any
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

                // Can reduce
                if (_children == null)
                {
                    // We don't have any children, so just shrink this node to the new size
                    // We already know that everything will still fit in it
                    SetValues(BaseLength / 2, _minSize, _looseness, _childBounds[bestFit].Center);
                    return this;
                }

                // No objects in entire octree
                if (bestFit == -1)
                {
                    return this;
                }

                // We have children. Use the appropriate child as the new root node
                return _children[bestFit];
            }

            /// <summary>
            /// Find which child node this object would be most likely to fit in.
            /// </summary>
            /// <param name="objBoundsCenter">The object's bounds center.</param>
            /// <returns>One of the eight child octants.</returns>
            public int BestFitChild(Point3 objBoundsCenter)
            {
                return (objBoundsCenter.X <= Center.X ? 0 : 1)
                       + (objBoundsCenter.Y >= Center.Y ? 0 : 4)
                       + (objBoundsCenter.Z <= Center.Z ? 0 : 2);
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

            // #### PRIVATE METHODS ####

            /// <summary>
            /// Set values for this node. 
            /// </summary>
            /// <param name="baseLengthVal">Length of this node, not taking looseness into account.</param>
            /// <param name="minSizeVal">Minimum size of nodes in this octree.</param>
            /// <param name="loosenessVal">Multiplier for baseLengthVal to get the actual size.</param>
            /// <param name="centerVal">Center position of this node.</param>
            private void SetValues(float baseLengthVal, float minSizeVal, float loosenessVal, Point3 centerVal)
            {
                BaseLength = baseLengthVal;
                _minSize = minSizeVal;
                _looseness = loosenessVal;
                Center = centerVal;
                _adjLength = _looseness * baseLengthVal;

                // Create the bounding box.
                Point3 size = new Point3(_adjLength, _adjLength, _adjLength);
                _bounds = new BBox(Center, size);

                float quarter = BaseLength / 4f;
                float childActualLength = (BaseLength / 2) * _looseness;
                Point3 childActualSize = new Point3(childActualLength, childActualLength, childActualLength);
                _childBounds = new BBox[8];
                _childBounds[0] = new BBox(Center + new Point3(-quarter, quarter, -quarter), childActualSize);
                _childBounds[1] = new BBox(Center + new Point3(quarter, quarter, -quarter), childActualSize);
                _childBounds[2] = new BBox(Center + new Point3(-quarter, quarter, quarter), childActualSize);
                _childBounds[3] = new BBox(Center + new Point3(quarter, quarter, quarter), childActualSize);
                _childBounds[4] = new BBox(Center + new Point3(-quarter, -quarter, -quarter), childActualSize);
                _childBounds[5] = new BBox(Center + new Point3(quarter, -quarter, -quarter), childActualSize);
                _childBounds[6] = new BBox(Center + new Point3(-quarter, -quarter, quarter), childActualSize);
                _childBounds[7] = new BBox(Center + new Point3(quarter, -quarter, quarter), childActualSize);
            }

            /// <summary>
            /// Private counterpart to the public Add method.
            /// </summary>
            /// <param name="obj">Object to add.</param>
            /// <param name="objBounds">3D bounding box around the object.</param>
            private void SubAdd(T obj, BBox objBounds)
            {
                // We know it fits at this level if we've got this far

                // We always put things in the deepest possible child
                // So we can skip some checks if there are children already
                if (!HasChildren)
                {
                    // Just add if few objects are here, or children would be below min size
                    if (_objects.Count < NumObjectsAllowed || (BaseLength / 2) < _minSize)
                    {
                        OctreeObject newObj = new OctreeObject { Obj = obj, Bounds = objBounds };
                        _objects.Add(newObj);
                        return; // We're done. No children yet
                    }

                    // Fits at this level, but we can go deeper. Would it fit there?
                    // Create the 8 children
                    if (_children == null)
                    {
                        Split();
                        if (_children == null)
                        {
                            Debug.WriteLine("Child creation failed for an unknown reason. Early exit.");
                            return;
                        }

                        // Now that we have the new children, see if this node's existing objects would fit there
                        for (int i = _objects.Count - 1; i >= 0; i--)
                        {
                            OctreeObject existingObj = _objects[i];
                            // Find which child the object is closest to based on where the
                            // object's center is located in relation to the octree's center
                            int bestFitChild = BestFitChild(existingObj.Bounds.Center);
                            // Does it fit?
                            if (Encapsulates(_children[bestFitChild]._bounds, existingObj.Bounds))
                            {
                                _children[bestFitChild].SubAdd(existingObj.Obj, existingObj.Bounds); // Go a level deeper					
                                _objects.Remove(existingObj); // Remove from here
                            }
                        }
                    }
                }

                // Handle the new object we're adding now
                int bestFit = BestFitChild(objBounds.Center);
                if (Encapsulates(_children[bestFit]._bounds, objBounds))
                {
                    _children[bestFit].SubAdd(obj, objBounds);
                }
                else
                {
                    // Didn't fit in a child. We'll have to it to this node instead
                    OctreeObject newObj = new OctreeObject { Obj = obj, Bounds = objBounds };
                    _objects.Add(newObj);
                }
            }

            /// <summary>
            /// Private counterpart to the public <see cref="Remove(T, BBox)"/> method.
            /// </summary>
            /// <param name="obj">Object to remove.</param>
            /// <param name="objBounds">3D bounding box around the object.</param>
            /// <returns>True if the object was removed successfully.</returns>
            private bool SubRemove(T obj, BBox objBounds)
            {
                bool removed = false;

                for (int i = 0; i < _objects.Count; i++)
                {
                    if (_objects[i].Obj.Equals(obj))
                    {
                        removed = _objects.Remove(_objects[i]);
                        break;
                    }
                }

                if (!removed && _children != null)
                {
                    int bestFitChild = BestFitChild(objBounds.Center);
                    removed = _children[bestFitChild].SubRemove(obj, objBounds);
                }

                if (removed && _children != null)
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
                float quarter = BaseLength / 4f;
                float newLength = BaseLength / 2;
                _children = new BoundsOctreeNode[8];
                _children[0] = new BoundsOctreeNode(
                    newLength,
                    _minSize,
                    _looseness,
                    Center + new Point3(-quarter, quarter, -quarter));
                _children[1] = new BoundsOctreeNode(
                    newLength,
                    _minSize,
                    _looseness,
                    Center + new Point3(quarter, quarter, -quarter));
                _children[2] = new BoundsOctreeNode(
                    newLength,
                    _minSize,
                    _looseness,
                    Center + new Point3(-quarter, quarter, quarter));
                _children[3] = new BoundsOctreeNode(
                    newLength,
                    _minSize,
                    _looseness,
                    Center + new Point3(quarter, quarter, quarter));
                _children[4] = new BoundsOctreeNode(
                    newLength,
                    _minSize,
                    _looseness,
                    Center + new Point3(-quarter, -quarter, -quarter));
                _children[5] = new BoundsOctreeNode(
                    newLength,
                    _minSize,
                    _looseness,
                    Center + new Point3(quarter, -quarter, -quarter));
                _children[6] = new BoundsOctreeNode(
                    newLength,
                    _minSize,
                    _looseness,
                    Center + new Point3(-quarter, -quarter, quarter));
                _children[7] = new BoundsOctreeNode(
                    newLength,
                    _minSize,
                    _looseness,
                    Center + new Point3(quarter, -quarter, quarter));
            }

            /// <summary>
            /// Merge all children into this node - the opposite of Split.
            /// Note: We only have to check one level down since a merge will never happen if the children already have children,
            /// since THAT won't happen unless there are already too many objects to merge.
            /// </summary>
            private void Merge()
            {
                // Note: We know children != null or we wouldn't be merging
                for (int i = 0; i < 8; i++)
                {
                    BoundsOctreeNode curChild = _children[i];
                    int numObjects = curChild._objects.Count;
                    for (int j = numObjects - 1; j >= 0; j--)
                    {
                        OctreeObject curObj = curChild._objects[j];
                        _objects.Add(curObj);
                    }
                }
                // Remove the child nodes (and the objects in them - they've been added elsewhere now)
                _children = null;
            }

            /// <summary>
            /// Checks if outerBounds encapsulates innerBounds.
            /// </summary>
            /// <param name="outerBounds">Outer bounds.</param>
            /// <param name="innerBounds">Inner bounds.</param>
            /// <returns>True if innerBounds is fully encapsulated by outerBounds.</returns>
            private static bool Encapsulates(BBox outerBounds, BBox innerBounds)
            {
                return outerBounds.Contains(innerBounds.Min) && outerBounds.Contains(innerBounds.Max);
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
                    foreach (BoundsOctreeNode child in _children)
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
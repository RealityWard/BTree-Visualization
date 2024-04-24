---

File Created: 2024-03-04

File Created Time: 14:47:42

Project: "[[Project B-Tree Visualization]]"

Hierarchical Level: Work Package

ID: "07"

Parent Note: "[[Team Reports B.TR]]"

WBS ID: "B.TR.WP07"

Primary Responsible:

  - Tristan Anderson 

Secondary Responsible: Emily Elzinga

Predecessor:

  - "[[B Tree Implementation Done Part 2 B.02.M17]]"

Status: Done

Start Date: 2024-03-04

End Date: 2024-04-04

Duration: 2d

Actual Duration: 6h

Estimate Duration: 3h

---

  

# B-Tree Description

## Properties of Nodes

This B-Tree structure defined within this program has the following properties:

- Each node has only one parent except root which has none.

- The tree has degree $T$, where $T$ is an integer greater than or equal to 2. The upper and lower bounds for the number of keys and children are defined with $T$. 

- Every node except root has a number of entries, $e$, such that $T-1 <= e <= 2T-1$. Root has $1 <= e <= 2T-1$.

- Every Non-Leaf node except root has a number of children, $c$, such that $T <= c <= 2T$. Root has $1 <= c <= 2T$.

- Entries are defined as key-value pairs and are sorted by keys from least to greatest.

- Children are sorted in relation to the parents keys, $k$, so that the keys of the child at index $i$, are $k_i < c_i < k_{i+1}$.

  

## Insertion

Insertion happens in the leaf nodes but starts at the root and runs recursively.
Time complexity: $O(log (n))$

1. A node takes a given key and searches amongst its entries, $k$, finding an index, $i$, that satisfies  $k_i < new \ key < k_{i+1}$.

2. If the node is a non-leaf it sends the insert down to its child at index $i$.

3. If the node is a leaf it inserts the new entry between $k_i$ and $k_{i+1}$.

4. Afterwards we check if the node has $2T-1$ entries.

5. If so then we split the node into nodes $A\ \text{and}\ B$ such that $k_0 <= A_k < k_T < B_k <= k_{2T-1}$. The remaining entry $k_T$ is returned to the parent.

6. The parent node then inserts the returned entry $k_T$ at index $i+1$ and stores $A_k$ at $i$ and $B_k$ at $i+1$.

7. Repeat steps 4-6 until the root node has been checked.

8. If root is needs to split we create a new non-leaf node and set it as the parent of the root node. Then proceed with steps 5 and 6. The new non-leaf node is now the root.

  

## Search

Searching for an entry by key is relatively straight forward. Proceeding the same as an insert except we stop when we find a match. $key = k$.
Time complexity: $O(log (n))$
  

## Deletion

Deleting a key-value pair can happen anywhere in the tree. This means its more involved.
Time complexity: $O(log (n))$

1. Proceed with a normal search. 

2. If the key is found in a leaf node; delete the key and shift the rest of the entries appropriately.

3. Afterwards the parent must check if the node has at least $T-1$ entries.

4. If the child node $k_i$ has less than $T-1$ entries:
	1. The parent gives the key-value pair $P_i$ that divides the child and its sibling, whether it be left or right, to the child.
	2. If the sibling node $k_{i+1}$ has $T$ entries or more, the sibling gives its lowest leftmost child to the parent node.
	3. Else, if the sibling node $k_{i-1}$ has $T$ entries or more, the sibling gives its highest rightmost child to the parent node.
	4. Else, the child node gives all of its keys to its left sibling.
	5. If there is no left sibling, the child node gives all its keys to its right sibling.
5. Continue propagating the changes up the tree this way, merging and borrowing keys when necessary.

# BPlus-Tree Description
## Properties of Nodes


The BPlus-Tree structure defined within this program has the following properties:
- Each node has only one parent except the root, which has none.

 - The tree has order $m$, which is an integer greater than or equal to 2. The upper and lower bounds for the number of keys and children are defined with $m$. 

- Every node except root has a number of entries, $e$, equal to $ceil(m/2) <= e <= m - 1$. Root has $1 <= e <= m - 1$.

- Every Non-Leaf node except root has a number of children, $c$, such that $ceil(m/2) + 1 <= c <= m$. Root has $2 <= c <= m$.

- A node is underflow if either the number of keys or the number of children are less than $ceil(m/2)$, or 2 in the root's case.  

- A node is overflow if either the number of keys or the number of children are more than $m$. 

- There is data stored only in the leaf nodes. The internal nodes are comprised of signpost key values showing where in the leaf node list a specific key-value pair is. 

- All leaf nodes have pointers to and from each other, making a doubly-linked-list.
- The keys in a node are sorted least to greatest. 

- Children are sorted in relation to the parents keys, $k$, so that the keys of the child at index $i$, are $k_i < c_i < k_{i+1}$.


## BPlus-Tree Insertion

1. Search the root's keys for the correct branch of the tree to insert the key $k_n$ into. 
2. Once a key $k$ is found that is greater than the key to be inserted, go to $k$'s child. Continue until a leaf node has been reached. 
3. Search the leaf node's keys until a key $k$ that is greater than $k_n$. Insert $k_n$ before $k$, moving $k$ and the keys after it one index down the keys array. 
4. If, after insertion, the $ceil(m/2) <= e <= m - 1$ property above is violated, the node must split in half. 
5. After splitting, the algorithm determines if the parent must split due to having too many children. If so, the new node gets all the keys in the right half. The children in the right also go to the new node. 
6. The above step is repeated until the root is reached. 

## BPlus-Tree Search

1. Search the root's keys for the correct branch of the tree in which to look for the given key $k_s$. Look for a key in the node's list that is greater than $k_s$. Go to the child of that key.
2. Traverse down the tree, looking on each level for the same criteria in step (1).
3. Search the keys of the leaf node. If the key is found, return the key and the data associated with it. Otherwise, return false. 


## BPlus-Tree Deletion

1. Start with a normal search for the given key, $k_d$. 
2. If the given key is not found, return false. 
3. Otherwise, delete $k_d$.
4. If the deletion has caused the node $n_i$ to be underflow, $n_i$ looks to its siblings.
	1. If the $n_{i-1}$ is not null (null means $n_i$ is on the leftmost edge of the tree) and has extra keys, it gives one to the current node. 
	2. Else, if the right sibling $n_{i+1}$ is not null and has extra keys, it gives one to the $n_i$.
5. If neither siblings can spare keys, $n_i$ tries first to merge with $n_{i+1}$. 
6. If the $n_{i+1}$ is null, $n_i$ merges with $n_{i-1}$.
7. The algorithm goes to the nodes' parent and determines if it is now underflow. If it is, merge using the directions described in steps (4-6).
  

# Successors

```dataview

list

from [[]]

where contains(predecessor, this.file.link)

sort wbs-id

```
/** @file ReaperNode.css
 *  @brief Implementation of the class ReaperNode
 *  @author David Hil
 *
 *  Copyright (c) 2019 David Hill
 *  
 *  Permission is hereby granted, free of charge, to any person obtaining a copy
 *  of this software and associated documentation files (the "Software"), to deal
 *  in the Software without restriction, including without limitation the rights
 *  to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
 *  copies of the Software, and to permit persons to whom the Software is
 *  furnished to do so, subject to the following conditions:
 *  
 *  The above copyright notice and this permission notice shall be included in all
 *  copies or substantial portions of the Software.
 *  
 *  THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
 *  IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
 *  FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
 *  AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
 *  LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
 *  OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
 *  SOFTWARE.
 */

using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Linq;
using System;

namespace UnityReaperParser
{

    public partial class ReaperNode
    {

// public:

        /// <summary>
        /// Initializes a new instance of the <see cref="T:ReaperNode"/> class.
        /// </summary>
        /// <param name="text">The current line as a string from the reaper file.</param>
        /// <param name="parent">The parent node. Pass null if there is no parent.</param>
        /// <param name="parser">The parsing instance of ReaperParser.</param>
        public ReaperNode(string text, ReaperNode parent, ReaperParser parser)
        {
            this.children = new List<ReaperNode>(); 
            this.parser = parser;                   
            this.parent = parent;                  
            text = text.Trim();                     

            var matches = Regex.Matches(text, "(\"[^\"]*\"|[\\S]+)");                 // split the values
            var matchList = matches.Cast<Match>().Select(match => match.Value).ToList();    // convert matches to string list
            matchList = matchList.Select(s => s.Trim(new char[] { ' ', '\"', '<', '>' })).ToList();



            /* 
             * Try to parse the first element of matches as a float. If it is a 
             * number, the node has no type but only values. if it is not a 
             * number, the first element is the type of the node.
             */
                          
            try
            {
                var num = float.Parse(matchList[0]);    
                type = "";                              
                values = matchList;                     
            }
            catch 
            {
                if (matchList.Count > 0)
                {
                    type = matchList[0];    
                    matchList.RemoveAt(0);
                    values = matchList;
                }
            }
        }

        /// <summary>
        /// Adds a child.
        /// </summary>
        /// <param name="o">O.</param>
        public void addChild(ReaperNode o)
        {
            children.Add(o);
        }



        /// <summary>
        /// The Reaper Parser that parsed the reaper file. That's the way to get the directory of the reaper session file.
        /// </summary>
        public readonly ReaperParser parser;

        /// <summary>
        /// The parent ReaperNode. If null, this node is the main ReaperSession Node.
        /// </summary>
        public readonly ReaperNode parent;

        /// <summary>
        /// The type of this Node as a string.
        /// </summary>
        public readonly string type;                                           

        /// <summary>
        /// The values of this Node.
        /// </summary>
        public readonly List<string> values;                                         

        /// <summary>
        /// Gets the first value of this nodes values.
        /// </summary>
        /// <value>The first value of this nodes values.</value>
        public string value { get { return values.Count > 0 ? values.First() : ""; } }        

        /// <summary>
        /// All Children of this ReaperNode as a instances of ReaperNode. 
        /// A child can be a 
        /// - TRACK
        /// - ITEM (a region on a track)
        /// - POSITION (which might be the position of an item on a track), 
        /// - FADEIN (which might be an array of numbers) or 
        /// - PT (which is a point of an envelope)
        /// - etc.
        /// There are many other properties and values. Have a look into
        /// the .rpp file (open in any text editor).
        /// </summary>
        /// <value>The child nodes of this node.</value>
        public List<ReaperNode> children { get; private set; }



		/// <summary>
		/// Gets all children of the given type with the given name
		/// </summary>
		/// <returns>All children of given type with the given name.</returns>
		/// <param name="type">Type.</param>
        /// <param name="name">Name.</param>
		/// <param name="recursive">Search child nodes too. False by default.</param>
		public List<ReaperNode> GetNodesByTypeAndName(string type, string name, bool recursive = false)
		{
			List<ReaperNode> nodes_ = new List<ReaperNode>();
			
			foreach (var n_ in GetNodes(type,recursive))
			{
				
				if (n_.GetNode("NAME").value == name)
					nodes_.Add(n_);
			}
			
			return nodes_;
		}
		
		/// <summary>
		/// Find a node by it's type and name.
		/// </summary>
		/// <returns>The node by type and name.</returns>
		/// <param name="type">Type.</param>
		/// <param name="name">Name.</param>
		public ReaperNode GetNodeByTypeAndName(string type, string name)
		{
			List<ReaperNode> nodes_ = new List<ReaperNode>();
			
			foreach (var n_ in GetNodesRecursive(type))
			{
				
				if (n_.GetNode("NAME").value == name)
					return n_;
			}
			
			return null;
		}
		
        /// <summary>
        /// Find a node by it's type.
        /// </summary>
        /// <returns>The first child node with type type.</returns>
        /// <param name="type">Type.</param>
        public ReaperNode GetNode(string type)
        {
            return children.Find(x => x.type == type);
        }

        /// <summary>
        /// Gets the last child node with type type.
        /// </summary>
        /// <returns>The last child node with type type.</returns>
        /// <param name="type">Type.</param>
        public ReaperNode GetLastNode(string type)
        {
            return children.FindLast(x => x.type == type);    
        }

        /// <summary>
        /// Gets all child nodes with type type.
        /// </summary>
        /// <returns>The child nodes with type type.</returns>
        /// <param name="type">Type.</param>
        /// <param name="recursive">If true search child nodes too. False by default.</param>
        public List<ReaperNode> GetNodes(string type, bool recursive = false){
            return !recursive ? children.FindAll(x => x.type == type) : GetNodesRecursive(type);   
        }


// private:

        /// <summary>
        /// Search all children recursively. Finds children in children too and so on...
        /// </summary>
        /// <returns>All children of type type.</returns>
        /// <param name="type">Type.</param>
        private List<ReaperNode> GetNodesRecursive(string type){
            List<ReaperNode> found = new List<ReaperNode>();
            GetAllChildrenRecursive(ref found, type);
            return found;
        }

        private void GetAllChildrenRecursive(ref List<ReaperNode> found, string type){
            List<ReaperNode> _children = GetNodes(type);
            found.AddRange(_children);
            foreach (var _c in _children){
                _c.GetAllChildrenRecursive(ref found, type);
            }
        }
    }
}
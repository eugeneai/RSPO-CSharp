using System;
using System.Collections.Generic;

namespace RSPO
{
	public class Analyzer<T>
	{
        // protected

		public Analyzer() { }

        public virtual bool Process()
        {
            return true;
        }

        public void Add(T row)
        {
            input.Add(row);
        }

        protected List<T> input = null;
	}

	public class ClusterAnalyzer<T> : Analyzer<T>
	{
		public ClusterAnalyzer() : base() { }

		public override bool Process()
		{
			return true;
		}

        protected alglib.clusterizerstate state;

	}


}

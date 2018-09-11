using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;

namespace RadDB3.structure {
	public class Set<T> : IEnumerable<T> {

		private LinkedList<T> list;


		public Set(params T[] items) {
			list = new LinkedList<T>(items);
		}

		public Set(ICollection<T> collection) {
			list = new LinkedList<T>(collection);
		}

		public Set(IEnumerable<T> enumerable) {
			list = new LinkedList<T>();
			foreach (T element in enumerable) {
				list.AddLast(element);
			}
		}

		IEnumerator IEnumerable.GetEnumerator() {
			return GetEnumerator();
		}

		public IEnumerator<T> GetEnumerator() {
			return list.GetEnumerator();
		}

		public static Set<T> operator +(Set<T> a, Set<T> b) {
			Set<T> set = new Set<T>(a.list);
			foreach (T t in b.list) {
				set.list.AddLast(t);
			}

			return set;
		}

		public static Set<T> operator +(T a, Set<T> b) {
			
			var set = new Set<T>(b);
			set.list.AddFirst(a);
			return set;
		}
		
		public static Set<T> operator +(Set<T> a, T b) {
			
			var set = new Set<T>(a);
			set.list.AddLast(b);
			return set;
		}
	}
}
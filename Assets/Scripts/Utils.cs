using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Linq;

public static class Utils {

	public static void ForEach<T> (this IEnumerable<T> collection, Action<T> func) {
		foreach (var item in collection) {
			func (item);
		}
	}
}

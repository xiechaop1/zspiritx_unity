using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class CoroutineQueue<T> : CustomYieldInstruction {
	public override bool keepWaiting {
		get {
			return queue.Count == 0;
		}
	}

	private Queue<T> queue;


	public CoroutineQueue(){
		queue = new Queue<T>();
	}
	public CoroutineQueue(int count) {
		queue = new Queue<T>(count);
	}
	public CoroutineQueue(IEnumerable<T> collection) {
		queue = new Queue<T>(collection);
	}

	public int Count {
		get { return queue.Count; }
	}

	public void Enqueue(T item) {
		queue.Enqueue(item);
	}
	public T Dequeue() {
		return queue.Dequeue();
	}
	public T Peek(T item) {
		return queue.Peek();
	}
	public T[] ToArray(){
		return queue.ToArray();
	}
}

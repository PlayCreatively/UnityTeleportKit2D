using System.Collections;
using UnityEngine;
using System;
using System.Runtime.InteropServices;
using System.Reflection;
using System.Collections.Generic;
using System.Linq;

namespace Essentials
{

	public enum Axis {x,y}
	static class EssentialFuncs
	{
		public class StaticMono : MonoBehaviour
		{
			static StaticMono thisMono;
			public static StaticMono Get
				=> thisMono != null ? thisMono : (thisMono = new GameObject("StaticMono").AddComponent<StaticMono>());
        }

		/// <summary>
		/// Add Action events to the ienumerator and run it with StartCoroutine.
		/// </summary>
        public static void AddEvents(this IEnumerator routine, [Optional] Action OnExit, [Optional] Action OnUpdate) 
			=> StaticMono.Get.StartCoroutine(CoroutineEvents(routine, OnExit, OnUpdate));

        static IEnumerator CoroutineEvents(IEnumerator routine, [Optional] Action OnExit, [Optional] Action OnUpdate)
		{
			while (routine.MoveNext())
			{
				OnUpdate?.Invoke();
				yield return routine.Current;
			}
			OnExit?.Invoke();
		}

		/// <summary>
		/// Ignores collision between col1 and col2 while they are toucing.
		/// </summary>
		public static void IgnoreCollision(Collider2D col1, Collider2D col2)
		=> StaticMono.Get.StartCoroutine(IgnoreCollisionRoutine(col1, col2));
		public static IEnumerator IgnoreCollisionRoutine(Collider2D col1, Collider2D col2)
		{
			Physics2D.IgnoreCollision(col1, col2, true);
			while (Physics2D.Distance(col1, col2).isOverlapped) yield return new WaitForFixedUpdate();
			Physics2D.IgnoreCollision(col1, col2, false);
		}

		public static T AddComponent<T>(this GameObject game, T duplicate) where T : Component
		{
			T target = game.AddComponent<T>();
			foreach (PropertyInfo x in typeof(T).GetProperties())
				if (x.CanWrite && x.Name != "density")
					x.SetValue(target, x.GetValue(duplicate));
			return target;
		}


		public static void StretchNSquash(this Transform trans, float value, Axis axis = 0)
		{
			float magnitude = trans.localScale.x + trans.localScale.y;
			var scale = trans.localScale;
			switch (axis)
			{

				case Axis.x:
					scale.x = magnitude * .5f * value;
					scale.y = magnitude - scale.x;
					break;
				case Axis.y:
					scale.y = magnitude * .5f * value;
					scale.x = magnitude - scale.y;
					break;
			}
			trans.localScale = scale;
		}
	}

	/// <summary>
	/// A binary Vector2
	/// </summary>
	public struct Binary
	{
		readonly Vector2 Value;

		public Binary(Vector2 value) : this()
		{
			if (value == Vector2.zero)
				Value = Vector2.zero;
			else if (Mathf.Abs(value.x) > Mathf.Abs(value.y))
				Value = new Vector2(Mathf.Sign(value.x), 0);
			else
				Value = new Vector2(0, Mathf.Sign(value.y));
		}

		public static implicit operator Binary(Vector2 value)
			=> new Binary(value);

		public static implicit operator Vector2(Binary value)
			=> value.Value;
	}

#pragma warning disable CS0660 // Type defines operator == or operator != but does not override Object.Equals(object o)
#pragma warning disable CS0661 // Type defines operator == or operator != but does not override Object.GetHashCode()
	/// <summary>
	/// Sign value of float
	/// </summary>
    public struct Sign
#pragma warning restore CS0661 // Type defines operator == or operator != but does not override Object.GetHashCode()
#pragma warning restore CS0660 // Type defines operator == or operator != but does not override Object.Equals(object o)
    {
		readonly float Value;

		public readonly static Sign positive = new Sign(1);
		public readonly static Sign negative = new Sign(-1);

		public Sign(float value) : this()
			=> Value = value < 0 ? -1 : 1;

		public static implicit operator Sign(float value)
			=> new Sign(value);

		public static implicit operator float(Sign value)
			=> value.Value;

		static bool IsPositive(float value) => value >= 0f;

		public static bool operator ==(float floatValue, Sign signValue)
			=> IsPositive(floatValue) == IsPositive(signValue.Value);
		public static bool operator !=(float floatValue, Sign signValue)
			=> IsPositive(floatValue) != IsPositive(signValue.Value);

		public static Sign operator -(Sign value)
			=> -value.Value;
	}

	/// <summary>
	/// Absolute value of float
	/// </summary>
	public struct Abs
	{
		private readonly float Value;

		public Abs(float value) : this()
			=> Value = value < 0 ? -value : value;

		public static implicit operator Abs(float value)
			=> new Abs(value);

		public static implicit operator float(Abs value)
			=> value.Value;
	}

	public enum Normalization { unlimited, clamp, smoothClamp }
	/// <summary>
	/// Timer that ticks towards a set time and can be used as a float: the timer's normalized progression.
	/// It can also be used as a bool: true if finished.
	/// </summary>
	public class Timer
	{
		public float Inverse => 1f-this;
		public float TargetTime { get; private set; }
		public float TimeStarted { get; private set; }
		/// <summary>
		/// Whether to clamp the progress or let it exceed 1. By default it's true.
		/// </summary>
		public Normalization normalization = Normalization.clamp;
		public bool Reached { get; private set; }

		/// <param name="time">The time to count towards.</param>
		/// <param name="clamp">Whether to clamp the progress or let it exceed 1. By default it's true</param>

		public Timer() { }

		public Timer(Normalization normalization)
			=> this.normalization = normalization;

		/// <summary>
		/// Start/restart the clock.
		/// </summary>
		public void Start(float setTime)
		{
			TimeStarted = Time.time;
			Reached = false;
			TargetTime = setTime;
		}

		/// <summary>
		/// Set the timer to have reached the goal.
		/// </summary>
		public void Finish() 
			=> Reached = true;

		public static implicit operator float(Timer timer)
		{
			float normalizedTime = (Time.time-timer.TimeStarted)/timer.TargetTime;

			switch (timer.normalization)
			{
				case Normalization.clamp:
					normalizedTime = Mathf.Clamp01(normalizedTime);
					break;
				case Normalization.smoothClamp:
					normalizedTime = Mathf.SmoothStep(0, 1, normalizedTime);
					break;
			}

			return normalizedTime;
		}

		public static implicit operator bool(Timer timer)
			=> timer.Reached || (timer.Reached = timer >= 1f);

		public static implicit operator Timer(bool boolValue)
			=> new Timer{Reached = boolValue};
			
	}

	//public struct VelocityVector2
	//{
	//	Vector2 point;
	//	Vector2 velocity;

	//	public Vector2 Update(float gravity, float drag, Vector2 pivotOffset)
	//	{
	//		pivotOffset *= 1f/5f;
	//		velocity += (-point + pivotOffset) * gravity * Time.deltaTime;
	//		point += velocity * Time.deltaTime;
	//		velocity += -velocity * drag * Time.deltaTime;

	//		Vector2 pointPos = -pivotOffset + point;
	//		if (pointPos.magnitude > 1f) pointPos.Normalize();
	//		if (velocity.magnitude > gravity)velocity = velocity.normalized * gravity;
	//		point = pivotOffset + pointPos;
	//		return pointPos;
	//	}
	//}

}
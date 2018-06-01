using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FPSCounter : Singleton<FPSCounter>
{
	public int HighestFPS
	{
		get; private set;
	}
    [SerializeField] private float _higestFPS;  // Use to show in inspector

	public int LowestFPS
	{
		get; private set;
	}
    [SerializeField] private float _lowestFPS;  // Use to show in inspector

	public int AverageFPS
	{
		get; private set;
	}
    [SerializeField] private float _averageFPS;  // Use to show in inspector

	public readonly int FrameRange = 60;

	private float[] _frameBuf;

	private int _curframe;
	
	private void InitFrameBuf()
	{
		int size = Mathf.Clamp(FrameRange, 1, 100);

		_frameBuf = new float[size];

		for(int i = 0; i < size; i++)
		{
			_frameBuf[i] = 1;
		}
	}

	/// <summary>
	/// Start is called on the frame when a script is enabled just before
	/// any of the Update methods is called the first time.
	/// </summary>
	private void Start()
	{
		InitFrameBuf();
	
		HighestFPS = LowestFPS = AverageFPS = 1;
	}

	/// <summary>
	/// Update is called every frame, if the MonoBehaviour is enabled.
	/// </summary>
	private void Update()
	{
		if(_frameBuf == null)
		{
			InitFrameBuf();
		}

		int size = _frameBuf.Length;
		float high = float.MinValue, low = float.MaxValue, sum = 0;

		_frameBuf[(_curframe++) % size] = 1f / Time.unscaledDeltaTime;

		for(int i = 0; i < size; i++)
		{
			sum += _frameBuf[i];
			
			if(high < _frameBuf[i])
			{
				high = _frameBuf[i];
			}
			if(low > _frameBuf[i])
			{
				low = _frameBuf[i];
			}
		}

		HighestFPS = (int)high;
		LowestFPS = (int)low;
		AverageFPS = (int)(sum / size); 

        _higestFPS = HighestFPS;
        _lowestFPS = LowestFPS;
        _averageFPS = AverageFPS;
	}
}
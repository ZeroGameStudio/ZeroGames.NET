// Copyright Zero Games. All Rights Reserved.

using System.Reflection;

namespace ZeroGames.Pooling;

public sealed class ObjectPool<T> where T : class, new()
{

	public ObjectPool(IConfigProvider<T> configProvider)
	{
		configProvider.GetConfig(out _config);
		for (int32 i = 0; i < _config.PrecacheCount; ++i)
		{
			_link.AddLast(new T());
		}
	}

	public T Get()
	{
		T instance = _link.First?.Value ?? new();
		if (_link.Count > 0)
		{
			_link.RemoveFirst();
		}

		// If client GetFromPool() throws then we throw.
		GetFromPool(instance);
		return instance;
	}

	public void Return(T instance)
	{
		if (_config.MaxAliveCount > 0 && _link.Count >= _config.MaxAliveCount)
		{
			return;
		}

		// If client ReturnToPool() throws then we throw at this point and won't add to link.
		ReturnToPool(instance);
		_link.AddLast(instance);
	}
	
	private void GetFromPool(T instance)
	{
		if (_intrusive)
		{
			((IPooled)instance).GetFromPool();
		}
		else
		{
			_getFromPoolMethod?.Invoke(instance, null);
		}
	}

	private void ReturnToPool(T instance)
	{
		if (_intrusive)
		{
			((IPooled)instance).ReturnToPool();
		}
		else
		{
			_returnToPoolMethod?.Invoke(instance, null);
		}
	}

	static ObjectPool()
	{
		_intrusive = typeof(T).IsAssignableTo(typeof(IPooled));
		if (!_intrusive)
		{
			MethodInfo[] methods = typeof(T).GetMethods(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
			foreach (var method in methods)
			{
				foreach (var attribute in method.GetCustomAttributes())
				{
					if (attribute is GetFromPoolAttribute)
					{
						_getFromPoolMethod = method;
					}
					if (attribute is ReturnToPoolAttribute)
					{
						_returnToPoolMethod = method;
					}
				}
			}
		}
	}

	private static readonly bool _intrusive;
	private static readonly MethodInfo? _getFromPoolMethod;
	private static readonly MethodInfo? _returnToPoolMethod;

	private readonly ObjectPoolConfig _config;
	private readonly LinkedList<T> _link = new();

}



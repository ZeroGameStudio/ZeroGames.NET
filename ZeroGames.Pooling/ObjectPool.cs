// Copyright Zero Games. All Rights Reserved.

using System.Reflection;

namespace ZeroGames.Pooling;

public sealed class ObjectPool<T> where T : class, new()
{

	public ObjectPool(IConfigProvider<T> configProvider)
	{
		configProvider.GetConfig(out _config);
		_storage = new(Math.Min(_config.MaxAliveCount, MAX_DEFAULT_CAPACITY));
		for (int32 i = 0; i < _config.PrecacheCount; ++i)
		{
			_storage.Enqueue(new T());
		}
	}

	public T Get()
	{
		T instance = _storage.TryDequeue(out var existing) ? existing : new();

		// If client GetFromPool() throws then we throw.
		GetFromPool(instance);
		return instance;
	}

	public void Return(T instance)
	{
		if (_config.MaxAliveCount > 0 && _storage.Count >= _config.MaxAliveCount)
		{
			return;
		}

		// If client ReturnToPool() throws then we throw at this point and won't add to link.
		ReturnToPool(instance);
		_storage.Enqueue(instance);
	}
	
	private void GetFromPool(T instance)
	{
		if (_intrusive)
		{
			((IPooled)instance).PreGetFromPool();
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
			((IPooled)instance).PreReturnToPool();
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
					if (attribute is PreGetFromPoolAttribute)
					{
						_getFromPoolMethod = method;
					}
					if (attribute is PreReturnToPoolAttribute)
					{
						_returnToPoolMethod = method;
					}
				}
			}
		}
	}

	private const int32 MAX_DEFAULT_CAPACITY = 1 << 10;

	private static readonly bool _intrusive;
	private static readonly MethodInfo? _getFromPoolMethod;
	private static readonly MethodInfo? _returnToPoolMethod;

	private readonly ObjectPoolConfig _config;
	private readonly Queue<T> _storage;

}



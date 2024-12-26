// Copyright Zero Games. All Rights Reserved.

namespace ZeroGames.Pooling;

public interface IConfigProvider<T> where T : class
{
	void GetConfig(out ObjectPoolConfig config);
}



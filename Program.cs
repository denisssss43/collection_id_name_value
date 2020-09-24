using System;
using System.Collections;
using System.Collections.Generic;
using System.Deployment.Internal;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Main
{
	/// <summary>
	/// Коллекция для тестового
	/// </summary>
	/// <typeparam name="ID">Тип id-части составного ключа элемента коллекции</typeparam>
	/// <typeparam name="NAME">Тип name-части составного ключа элемента коллекции</typeparam>
	/// <typeparam name="T">Тип значения элемента коллекции</typeparam>
	public class Collection<ID, NAME, T> : ICollection
		where ID: struct
		where NAME: struct
	{
		public Collection()
		{
			this.arr = new Dictionary<KeyValuePair<ID, NAME>, T>();
		}
		public Collection(List<KeyValuePair<KeyValuePair<ID, NAME>, T>> arr)
		{
			arr.ForEach(i => Add(i.Key.Key, i.Key.Value, i.Value));
		}
		public Collection(List<SCollectionElement> arr)
		{
			arr.ForEach(i => Add(i.id, i.name, i.value));
		}

		private Dictionary<KeyValuePair<ID, NAME>, T> arr = new Dictionary<KeyValuePair<ID, NAME>, T>();

		public T this[ID id, NAME name]
		{
			get
			{
				try
				{
					return arr[new KeyValuePair<ID, NAME>(id, name)];
				}
				catch (Exception e)
				{
					throw new InvalidOperationException(e.Message, e);
				}
			}
			set
			{
				try
				{
					arr[new KeyValuePair<ID, NAME>(id, name)] = value;
				}
				catch (Exception e)
				{
					throw new InvalidOperationException(e.Message, e);
				}
			}
		}

		public int Count => arr.Count;

		public bool IsSynchronized => false;
		public object SyncRoot => this;
		public IEnumerator GetEnumerator()
		{
			lock (arr)
			{
				return new Enumerator<SCollectionElement>(
				arr.Select(
					i => new SCollectionElement
					{
						id = i.Key.Key,
						name = i.Key.Value,
						value = i.Value,
					}
				).ToArray());
			}
		}
		public void CopyTo(Array array, int index)
		{
			lock (arr)
			{
				try
				{
					arr.ToList().ForEach(i => array.SetValue(i.Value, index++));
				}
				catch (Exception e)
				{
					throw new InvalidOperationException(e.Message, e);
				}
			}
		}

		/// <summary>
		/// Добавление нового элемента в коллекцию
		/// </summary>
		/// <param name="id">id-часть составного ключа элемента коллекции</param>
		/// <param name="name">name-часть составного ключа элемента коллекции</param>
		/// <param name="obj">Значение элемента коллекции</param>
		public void Add(ID id, NAME name, T obj)
		{
			lock (arr)
			{
				try
				{
					arr.Add(new KeyValuePair<ID, NAME>(id, name), obj);
				}
				catch (Exception e)
				{
					throw new InvalidOperationException(e.Message, e);
				}
			}
		}
		/// <summary>
		/// Добавление всех элементов коллекции в конец текущей коллекции
		/// </summary>
		/// <param name="collection">Добавляемая колекция</param>
		public void AddRange(Collection<ID, NAME, T> collection)
		{
			try
			{
				foreach (SCollectionElement i in collection)
					Add(i.id, i.name, i.value);
			}
			catch (Exception e)
			{
				throw new InvalidOperationException(e.Message, e);
			}
		}
		/// <summary>
		/// Получение списка элементов по части ключа
		/// </summary>
		/// <typeparam name="A">Тии ключа в соответствии с типом данных указанных для ключа при инициализации коллекции</typeparam>
		/// <param name="searchType"></param>
		/// <param name="key"></param>
		/// <returns></returns>
		public Collection<ID, NAME, T> GetCollectionByKey<A>(EKeySearchType searchType, A key) where A : struct
		{
			lock (arr)
			{
				try 
				{
					switch (searchType)
					{
						case EKeySearchType.Id:
							return new Collection<ID, NAME, T>(arr.Where(i => i.Key.Key.Equals(key)).ToList());

						case EKeySearchType.Name:
							return new Collection<ID, NAME, T>(arr.Where(i => i.Key.Value.Equals(key)).ToList());

						default:
							throw new InvalidOperationException("Использован не существующий EKeySearchType.");
					}
				}
				catch (Exception e)
				{
					throw new InvalidOperationException(e.Message, e);
				}
			}
		}
		/// <summary>
		/// Отчистка коллекции
		/// </summary>
		public void Clear()
		{
			lock (arr)
			{
				try
				{
					arr.Clear();
				}
				catch (Exception e)
				{
					throw new InvalidOperationException(e.Message, e);
				}
			}
		}

		/// <summary>
		/// Получение коллекции List
		/// </summary>
		/// <returns></returns>
		public List<SCollectionElement> ToList()
		{
			lock (arr)
			{
				return arr.Select(i => new SCollectionElement
				{
					id = i.Key.Key,
					name = i.Key.Value,
					value = i.Value
				}).ToList();
			}
		}

		/// <summary>
		/// Определяет, содержит ли последовательность какие-либо элементы
		/// </summary>
		/// <returns></returns>
		public bool Any()
		{
			lock (arr)
			{
				return arr.Any();
			}
		}

		public class Enumerator<T> : IEnumerator<T>
		{
			private T[] arr;
			private int cursor;

			public Enumerator(T[] arr)
			{
				this.arr = arr;
				this.cursor = -1;
			}

			public T Current
			{
				get
				{
					if ((cursor < 0) || (cursor == arr.Length))
						throw new InvalidOperationException();
					return arr[cursor];
				}
			}

			object IEnumerator.Current
			{
				get
				{
					if ((cursor < 0) || (cursor == arr.Length))
						throw new InvalidOperationException();
					return arr[cursor];
				}
			}

			public bool MoveNext()
			{
				if (cursor < arr.Length)
					cursor++;

				return !(cursor == arr.Length);
			}

			public void Reset()
			{
				cursor = -1;
			}

			void IDisposable.Dispose() { }
		}
		/// <summary>
		/// Елемент коллекции
		/// </summary>
		public struct SCollectionElement
		{
			public ID id;
			public NAME name;
			public T value;
		}
	}
	/// <summary>
	/// Тип ключа по которому будет осуществляться поиск
	/// </summary>
	public enum EKeySearchType { Id, Name }
}

namespace Main
{
	/// <summary>
	/// Пользовательский тип для проверки работы с ними
	/// </summary>
	struct User
	{
		public string name;
		public string first;
	}

	class Program
	{
		static void Main(string[] args)
		{
			// объявил коллекцию
			Collection<int, User, string> collection = new Collection<int, User, string>();

			// наполнил ее 
			collection.Add(1, new User { name = "ДА", first = "НЕТ" }, "Привет");
			collection.Add(1, new User { name = "ДА", first = "ДА" }, "Привет");
			collection.Add(1, new User { name = "НЕТ" }, "Привет");
			collection.Add(2, new User { name = "ДА" }, "Привет");
			collection.Add(2, new User { name = "НЕТ" }, "Привет"); 


			// вывод по ключу Name 
			collection.GetCollectionByKey(EKeySearchType.Name, new User { name = "ДА" }).ToList().ForEach(i => 
				Console.WriteLine(string.Format("ID:{0,-8} NAME:{1,-8:f4} VALUE:{2,-8:f4}\n", i.id.ToString(), i.name.name.ToString(), i.value.ToString()))
			);

			Console.ReadKey();
		}
	}
}

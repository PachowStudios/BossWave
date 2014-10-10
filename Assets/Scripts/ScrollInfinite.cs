using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public class ScrollInfinite : MonoBehaviour 
{
	public float speed = 17.5f;
	public bool loop = false;

	private List<Transform> layers = new List<Transform>();

	void Awake()
	{
		for (int i = 0; i < transform.childCount; i++)
		{
			Transform child = transform.GetChild(i);
			
			if (child.renderer != null)
			{
				layers.Add(child);
			}

			layers = layers.OrderBy(t => t.position.x).ToList();
		}
	}

	void FixedUpdate()
	{
		if (loop)
		{
			transform.Translate(new Vector2(-speed, 0) * Time.deltaTime);

			Transform firstChild = layers.FirstOrDefault();

			if (firstChild != null)
			{
				if (firstChild.position.x < Camera.main.transform.position.x)
				{
					if (!firstChild.renderer.IsVisibleFrom(Camera.main))
					{
						Transform lastChild = layers.LastOrDefault();
						Vector3 lastSize = lastChild.renderer.bounds.max - lastChild.renderer.bounds.min;

						firstChild.position = new Vector3(lastChild.position.x + lastSize.x,
														  firstChild.position.y,
														  firstChild.position.z);

						layers.Remove(firstChild);
						layers.Add(firstChild);
					}
				}
			}
		}
	}
}

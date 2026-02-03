
using CockleBurs.GameFramework.Utility;
using Netick;
using Netick.Unity;
using UnityEngine;


namespace CockleBurs.GameFramework.Core
{
public partial class NetworkCherry : NetworkBehaviour
{ 
	public NetworkPlayer NetworkPlayer =>  Sandbox.GetPlayerById(this.InputSourcePlayerId);
	
	private Transform m_transform;
	private RectTransform m_rectTransform;
	private GameObject m_gameObject;
	public NetworkConnection  localConnection ;
	// Serialized Fields
	[SerializeField]
	[HideInInspector]
	private bool m_ShowProperties = false;
		
	// Properties
	/// <summary>
	/// Defines if the SerializedProperties of this SuperBehaviour should be shown in the inspector.
	/// </summary>
	public bool ShowProperties { get => m_ShowProperties; set => m_ShowProperties = value; }
	
	/// <summary>
	/// This is a cached version of the transform property.
	/// </summary>
	public new Transform transform
	{
		get
		  { 
			  if (IsDestroyed) return null;
			  try
				{
					if (m_transform == null)
					{
						m_transform = base.transform;
					}
					return m_transform;
				}
				catch
				{
					// In case unity uses they're null override to hide the object still exists
					return null;
				}
		  }
		}
		
		/// <summary>
        /// The GameObject attached to this SuperBehaviour.<br/>
        /// This is a cached version of the gameObject property.
        /// </summary>
		public new GameObject gameObject
		{
			get
			{
				if (IsDestroyed) return null;
				try
				{
					if (m_gameObject == null)
					{
						m_gameObject = base.gameObject;
					}
					return m_gameObject;
				}
				catch
				{
					// In case unity uses they're null override to hide the object still exists
					return null;
				}
			}
		}
		
		/// <summary>
        /// Returns if this instance has been marked as destroyed by Unity.
        /// </summary>
        /// <returns>True if the instance has been destroyed, false otherwise.</returns>
		public bool IsDestroyed => this == null;
		
        
	
        public virtual void OnActive(NetworkConnection connection) { }

        public float NetworkScaledFixedDeltaTime => Sandbox.ScaledFixedDeltaTime;

        public ReplicatedObject ReplicatedObject => (ReplicatedObject)Object;
        public NetworkObject NetworkObject => Object;
	
        

    
        protected virtual void Tick(float deltaTime)
        {
            
        }
}

}
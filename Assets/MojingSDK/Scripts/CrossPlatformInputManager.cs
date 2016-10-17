using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using MojingSample.CrossPlatformInput.PlatformSpecific;

namespace MojingSample.CrossPlatformInput
{
    public partial class CrossPlatformInputManager : MonoBehaviour
    {

        public static CrossPlatformInputManager main;

        public bool useCrossInput = true;
        [SerializeField]
        protected MonoBehaviour[] m_Sources = new MonoBehaviour[0];
        public List<InputSource> sources;

        [System.NonSerialized]
        public int numSources;
        protected WaitForEndOfFrame _waitEof;

        public static int timestamp;

        protected virtual void Awake()
        {
            if (main != null)
            {
                if (main != this)
                {
                    return;
                }
            }

            if (!useCrossInput)
            {
                virtualInput = new StandaloneInput();
                Destroy(this);
                return;
            }

            virtualInput = new MobileInput();
            main = this;
            DontDestroyOnLoad(this);

            InputSource source;
            numSources = 0;
            int i = 0, imax = m_Sources.Length;
            sources = new List<InputSource>(imax);
            for (; i < imax; ++i)
            {
                // Fix gameObject...
                if (m_Sources[i] == null || !m_Sources[i].gameObject.activeInHierarchy)
                    continue;
                source = m_Sources[i] as InputSource;
                if (source != null)
                {
                    if (source.enabled)
                    {
                        if (source.InitInput() == 0)
                        {
                            sources.Add(source);
                            numSources++;
                        }
                    }
                }
            }
            PrintLogOnStartup();
            _waitEof = new WaitForEndOfFrame();
            StartCoroutine(UpdateOnEof());
        }

        // Update is called once per frame
        protected virtual void Update()
        {
            timestamp = Time.frameCount;
            for (int i = 0; i < numSources; ++i)
            {
                sources[i].EnterInputFrame();
            }
        }

        protected virtual IEnumerator UpdateOnEof()
        {
            while (enabled)
            {
                yield return _waitEof;
                for (int i = 0; i < numSources; ++i)
                {
                    sources[i].ExitInputFrame();
                }
            }
        }

        protected virtual void OnDestroy()
        {
            timestamp = Time.frameCount;
            for (int i = 0; i < numSources; ++i)
            {
                sources[i].ExitInput();
            }
        }

        protected virtual void PrintLogOnStartup()
        {
            System.Text.StringBuilder sb = new System.Text.StringBuilder();
            sb.AppendLine(numSources.ToString());
        }
    }
}
/*
Copyright (C) 2025 GGGravity
https://github.com/sevvy-wevvy/Several-Bees/

This program is free software: you can redistribute it and/or modify
it under the terms of the GNU General Public License as published by
the Free Software Foundation, either version 3 of the License, or
(at your option) any later version.

This program is distributed in the hope that it will be useful,
but WITHOUT ANY WARRANTY; without even the implied warranty of
MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
GNU General Public License for more details.
*/

using UnityEngine;
using System;
using System.Reflection;
using System.Collections;

namespace SeveralBees.Scripts
{
    public class MachineDespawn : MonoBehaviour
    {
        public float DespawnDistance = Config.MachineDespawnDistance;
        bool despawning;

        void Update()
        {
            if (despawning) return;

            if (Vector3.Distance(Config.BodyReference().position, transform.position) > DespawnDistance)
            {
                despawning = true;
                SeveralBeesCore.Instance.PlaySound("https://github.com/sevvy-wevvy/Several-Bees/raw/refs/heads/main/Resources/Mod/close.wav");
                if (Api.Instance.GrabButton("8", "Animations").enabled) StartCoroutine(BounceOutAndDestroy());
                else Destroy(gameObject);
            }
        }

        IEnumerator BounceOutAndDestroy()
        {
            Vector3 ogScale = transform.localScale;
            Vector3 overshootScale = ogScale * 1.1f;

            float time = 0f;
            float duration = 0.1f;

            while (time < duration)
            {
                time += Time.deltaTime;
                float t = time / duration;
                transform.localScale = Vector3.Lerp(ogScale, overshootScale, t);
                yield return null;
            }

            time = 0f;
            duration = 0.2f;

            while (time < duration)
            {
                time += Time.deltaTime;
                float t = time / duration;
                t = Mathf.Sin(t * Mathf.PI * 0.5f);
                transform.localScale = Vector3.Lerp(overshootScale, Vector3.zero, t);
                yield return null;
            }

            Destroy(gameObject);
        }
    }
}


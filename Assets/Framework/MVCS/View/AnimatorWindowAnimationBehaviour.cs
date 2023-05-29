using UnityEngine;
using System.Collections;

namespace Framework
{
    public class AnimatorWindowAnimationBehaviour : WindowAnimationBehaviour
    {
        [System.NonSerialized] // 不持之Anmator
        public RuntimeAnimatorController _controller;
        public AnimationClip _openClip = null;
        public AnimationClip _loopClip = null;
        public AnimationClip _closeClip = null;
        public float _openSpeed = 1;
        public float _loopSpeed = 1;
        public float _closeSpeed = 1;

        private int _openID = Animator.StringToHash("open");
        private int _loopID = Animator.StringToHash("loop");
        private int _closeID = Animator.StringToHash("close");

        private Animator _animator;
        public virtual Animator SampleAnimator
        {
            get
            {
                if (_animator == null)
                {
                    _animator = GetComponent<Animator>();
                    if (_animator == null)
                    {
                        _animator = gameObject.AddComponent<Animator>();
                    }
                    _animator.enabled = false;
                    _animator.runtimeAnimatorController = _controller;
                }
                return _animator;
            }
        }

        private Animation _animation;
        public virtual Animation SampleAnimation
        {
            get
            {
                if (_animation == null)
                {
                    _animation = GetComponent<Animation>();
                    if (_animation == null)
                    {
                        _animation = gameObject.AddComponent<Animation>();
                    }
                    _animation.enabled = false;
                    if (_openClip != null)
                    {
                        _animation.AddClip(_openClip, "open");
                    }
                    if (_loopClip != null)
                    {
                        _animation.AddClip(_loopClip, "loop");
                    }
                    if (_closeClip != null)
                    {
                        _animation.AddClip(_closeClip, "close");
                    }
                }
                return _animation;
            }
        }

        private IEnumerator CoCreateAnimation(bool isOpen, System.Action done)
        {
            _controller = null;
            if (_controller != null)
            {
                var animator = SampleAnimator;

                var ok = false;
                if (animator)
                {
                    animator.enabled = false;

                    if (isOpen)
                    {
                        if (animator.HasState(0, _openID))
                        {
                            try
                            {
                                animator.Rebind();
                                animator.Play(_openID, 0, 0);
                                animator.Update(0);
                                ok = true;
                            }
                            catch (System.Exception e)
                            {
                                Debug.LogError(e);
                            }

                            while (ok && _controller != null && animator != null && animator.GetCurrentAnimatorStateInfo(0).normalizedTime < 1.0f)
                            {
                                animator.Update(Time.deltaTime);
                                yield return null;
                            }

                            done();

                            if (animator.HasState(0, _loopID))
                            {
                                try
                                {
                                    animator.Rebind();
                                    animator.Play(_loopID, 0, 0);
                                    animator.Update(0);
                                    animator.enabled = true;
                                }
                                catch (System.Exception e)
                                {
                                    Debug.LogError(e);
                                }
                            }

                            yield break;
                        }
                    }
                    else
                    {
                        if (animator.HasState(0, _closeID))
                        {
                            try
                            {
                                animator.Play(_closeID, 0, 0);
                                animator.Update(0);
                                ok = true;
                            }
                            catch (System.Exception e)
                            {
                                Debug.LogError(e);
                            }

                            while (_controller != null && animator != null && animator.GetCurrentAnimatorStateInfo(0).normalizedTime < 1.0f)
                            {
                                animator.Update(Time.deltaTime);
                                yield return null;
                            }

                            done();

                            yield break;
                        }
                    }
                }
            }
            else
            {
                var canvas = GetComponent<Canvas>();
                var animation = SampleAnimation;

                if (animation)
                {
                    animation.enabled = false;

                    if (isOpen)
                    {
                        if (_openClip != null)
                        {
                            animation.Play("open", PlayMode.StopAll);
                            animation.Sample();
                            var state = animation["open"];

                            if (state)
                            {
                                state.normalizedTime = 0;
                                while (animation != null && state.normalizedTime < 1.0f)
                                {
                                    state.time += _openSpeed * Time.deltaTime;
                                    animation.Sample();
                                    yield return null;
                                }
                            }
                        }

                        done();

                        if (_loopClip != null)
                        {
                            animation.Play("loop", PlayMode.StopAll);
                            animation.Sample();
                            var state = animation["loop"];

                            if (state)
                            {
                                state.normalizedTime = 0;
                                while (animation != null)
                                {
                                    state.time += _loopSpeed * Time.deltaTime;
                                    animation.Sample();
                                    yield return null;
                                }
                            }
                        }

                        yield break;
                    }
                    else
                    {
                        if (_closeClip != null)
                        {
                            animation.Play("close", PlayMode.StopAll);
                            animation.Sample();
                            var state = animation["close"];

                            if (state)
                            {
                                state.normalizedTime = _closeSpeed < 0 ? 1 : 0;
                                while (animation != null && (_closeSpeed < 0 ? state.normalizedTime > 0.0f : state.normalizedTime < 1.0f))
                                {
                                    state.time += _closeSpeed * Time.deltaTime;
                                    animation.Sample();
                                    yield return null;
                                }
                            }
                        }
                    }
                }
            }
            done();
        }

        public override void OnOpen(System.Action callback)
        {
            StopAllCoroutines();
            StartCoroutine(CoCreateAnimation(true, callback));
        }

        public override void OnClose(System.Action callback)
        {
            StopAllCoroutines();
            StartCoroutine(CoCreateAnimation(false, callback));
        }

        private void OnClearController()
        {
            _controller = null;
        }
    }
}

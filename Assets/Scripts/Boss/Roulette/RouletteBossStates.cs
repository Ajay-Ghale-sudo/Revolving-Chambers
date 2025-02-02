using State;
using UnityEngine;

namespace Boss.Roulette
{
    public class RouletteIdleState : BaseState<RouletteBoss>
    {
        public RouletteIdleState(RouletteBoss owner) : base(owner)
        {
            
        }

        public override void OnEnter()
        {
            base.OnEnter();
        }

        public override void OnExit()
        {
            base.OnExit();
        }

        public override void Update()
        {
            base.Update();
        }

        public override void FixedUpdate()
        {
            base.FixedUpdate();
        }
    }
    
    public class RouletteSingleAttackState : BaseState<RouletteBoss>
    {
        
        private float _attackRate = 1f;
        private float _attackTimer = 0f;
        
        public RouletteSingleAttackState(RouletteBoss owner) : base(owner)
        {
            
        }

        public override void OnEnter()
        {
            base.OnEnter();
        }

        public override void OnExit()
        {
            base.OnExit();
        }

        public override void Update()
        {
            _attackTimer += Time.deltaTime;
            if (!(_attackTimer >= _attackRate)) return;
            _attackTimer = 0f;
            _owner.ShootAtTarget();
        }

        public override void FixedUpdate()
        {
            base.FixedUpdate();
        }
    }
    
    public class RouletteSpreadAttackState : BaseState<RouletteBoss>
    {
        private float _attackRate = 2f;
        private float _attackTimer = 0f;
        
        public RouletteSpreadAttackState(RouletteBoss owner) : base(owner)
        {
            
        }

        public override void OnEnter()
        {
            base.OnEnter();
            _owner.centerWheel.StopWheel();
        }

        public override void OnExit()
        {
            base.OnExit();
        }

        public override void Update()
        {
            _attackTimer += Time.deltaTime;
            if (!(_attackTimer >= _attackRate)) return;
            _attackTimer = 0f;
            _owner.ShootSpreadShot();
        }

        public override void FixedUpdate()
        {
            base.FixedUpdate();
        }
    }
    
    public class RouletteHazardsState : BaseState<RouletteBoss>
    {
        private float _hazardRate = 10f;
        private float _hazardTimer = 0f;
        
        public RouletteHazardsState(RouletteBoss owner) : base(owner)
        {
            
        }

        public override void OnEnter()
        {
            base.OnEnter();
            _owner.leftWheel.StopWheel();
            _owner.ActivateHazards();
        }

        public override void OnExit()
        {
            base.OnExit();
        }

        public override void Update()
        {
            _hazardTimer += Time.deltaTime;
            if (!(_hazardTimer >= _hazardRate)) return;
            _hazardTimer = 0f;
            Trigger();
        }

        private void Trigger()
        {
            if (_owner.HealthPercentage < .5f) _owner.SpawnHazard();
            if (_owner.HealthPercentage < .75f) _owner.ActivateHazards();
        }

        public override void FixedUpdate()
        {
            base.FixedUpdate();
        }
    }
    
    public class RouletteDeadState : BaseState<RouletteBoss>
    {
        public RouletteDeadState(RouletteBoss owner) : base(owner)
        {
            
        }

        public override void OnEnter()
        {
            base.OnEnter();
            _owner.rightWheel.StopWheel();
            _owner.NextPhase();
        }

        public override void OnExit()
        {
            base.OnExit();
        }

        public override void Update()
        {
            base.Update();
        }

        public override void FixedUpdate()
        {
            base.FixedUpdate();
        }
    }
}
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace LibrarySystem
{
    public class EnumLibrary
    {
        //Modified by buffing/debuffing.
        internal enum ImpactableStats
        {
            Vitality,
            MovementSpeed



        }



        internal enum Elements
        {
            Metallic, //Physical material
            Electrical, //Lightning/EMP
            HighThermal, //Plasma, Explosive
            MidThermal, //Fire, Laser
            LowThermal, //Ice, Stun
            Radioactive
        }

        internal enum Aspects
        {
            Immune,
            Flux,
            Gravity,
            Magnetism,
            Probability,
            Density,
            Visibility


        }

        internal enum Rarity
        {
            Common,
            Uncommon,
            Rare,
            Epic,
            Legendary,
            Mythical
        }

        internal enum TargetingType
        {
            FriendlyEntities,
            HostileEntities
        }

        internal enum TargetAcquisitionMethod
        {
            ForwardFromPoint,
            Closest,
            Farthest,
            FromPlayerList

        }


        internal enum MachineType
        {
            Sentry,
            Despect,
            Internic,
            Sigion
        }

        internal enum EntityState
        {
            Attacking,
            Defending,
            Special
        }



        internal enum EnhancerType
        {
            MovementSpeedBoost,
            SprintBoost,
            AttackSpeed,
            JumpCount,
            DashCount,
            MaxSentries,
            MaxProxies,
            BlockChance,
            CriticalHitChance,
            BossDamageBoost,
            VitalityRegen,
            NaniteRegen,
            LuckFactor,
            CooldownDash,
            CooldownAbility,
            CooldownUltimate,
            Lives
        }
        #region BLUEPRINT ENUMS
        internal enum BlueprintModuleType
        {
            SingleUse,
            Sentry,
            Proxy
        }

        internal enum BlueprintModuleCategory //Different categories show up as a specific color in the hud. Damage = red, healing = green, shielding = blue, etc.
        {
            Damaging, //Red
            Healing, //Green
            Shielding, //Blue
            Buffing, //Yellow
            Debuffing, //Pink
            Special //Purple
        }

        internal enum BlueprintModuleObjectType
        {
            MetalSphere,
            PlasmaSphere,
            LightningSphere,
            FireSphere,
            MetalShard
        }

        internal enum BlueprintModuleStyle
        {
            Origin,
            MovingObject,
            Effect
        }

        internal enum NewStuff_BlueprintModuleOutputType
        {
            Point, //Output is at an array of points. 
            Forward, //Output is using the "forward array" from the blueprint interface or individual machine/proxy/sentry scripts.
            Downward, //Output is using the "downward array" from the blueprint interface or individual machine/proxy/sentry scripts. 
            
        }

        internal enum BlueprintModuleDamagingStyle
        {
            Origin, //Self inflicts damage...for some reason.
            Forward, //Fire something forward
            Downward, //Fire something downward over a atarget
            ConstantSphere, //Surrounding self
            Burst //Explodes from origin. Explosions on contact can be done using additional rules
        }

        internal enum BlueprintModuleHealingStyle
        {
            Origin, //Heals self
            Forward, //Fires a healing attack at a friendly target.
            Downward, //Rains down whatever the 'attack' is on a friendly target as a heal.
            ConstantSphere, //Creates a healing sphere that heals friendlies in the AOE
            Burst
        }

        internal enum BlueprintModuleShieldingStyle
        {
            Origin, //Gives temporary armor to player ~~!!!! MAYBE OR MAYBE NOT GONNA USE THIS
            Forward, //Makes a forward facing shield.
            ConstantCircle, //Makes a moving shield that orbits the player.
            ConstantSphere, //Makes a barrier around the player
            AtCrosshairHit //Creates a barrier at the hitpoint. Can be a wall,  a bubble, etc.
        }

        internal enum BlueprintModuleBuffingStyle
        {
            Origin, //Just this entity.
            Forward, //Buffs friendly target
            Downward, //Buffs friendly target with a downward effect
            ConstantSphere, //Buffs friendlies in sphere of influence
            Burst //Buffs nearby friendlies
        }

        internal enum BlueprintModuleDebuffingStyle
        {
            Origin, //Debuff just this entity...for some reason..
            Forward, //Fires a debuff attack forward
            Downward, //Fires a debuff attack downward
            ConstantSphere, //Debuffs enemies in sphere of influence
            Burst //Debuffs nearby enemies
        }

        internal enum BlueprintModuleSpecialStyle 
        {
            Origin,
            Forward,
            ConstantSphere,
            Burst,
            ActiveTarget //Special: A special move where players can grab enemies etc will rely on this.
        }

        internal enum AbilityMovementType
        {
            Straight,
            Seeking
        }

        #endregion

    }
}
﻿using MBC.Core.Accolades;
using MBC.Core.Events;
using MBC.Core.Matches;
using MBC.Shared;
using System;
using System.Collections.Generic;

namespace MBC.Core.Rounds
{
    /// <summary>
    /// Manipulates the information in provided <see cref="ControllerRegister"/>s to reflect the state of a
    /// round of battleship through events that are generated by deriving classes. <see cref="ControllerRegister"/>s
    /// should not be modified outside the scope of a deriving round class as each round depends on the state
    /// of each <see cref="ControllerRegister"/> to be modified only by the class; instead a copy should be
    /// made if modifications are desirable.
    /// </summary>
    public abstract class Round
    {
        private AccoladeGenerator accoladeGenerator;

        private EventIterator events;

        private List<Accolade> generatedAccolades;

        /// <summary>
        /// Attaches the <see cref="MatchInfo"/> from a <see cref="Match"/> and attaches the
        /// <see cref="ControllerRegister"/>s given. Initializes an empty list of <see cref="Round.Events"/>
        /// and <see cref="Round.Accolades"/>.
        /// </summary>
        /// <param name="inputRegisters">A variable number of controllers that are involved in this Round.</param>
        /// <param name="matchInfo">Information about the match that determines Round behaviour.</param>
        public Round(MatchInfo matchInfo, List<ControllerRegister> inputRegisters)
        {
            MatchInfo = matchInfo;

            events = new EventIterator();
            Registers = new List<ControllerRegister>();
            Remaining = new List<ControllerID>();
            generatedAccolades = new List<Accolade>();
            accoladeGenerator = new AccoladeGenerator(this);

            var idRandomizeList = new List<ControllerID>();
            var idRandom = new Random();
            foreach (var register in inputRegisters)
            {
                Registers.Add(register);
                idRandomizeList.Add(register.ID);
            }
            while (idRandomizeList.Count > 0)
            {
                Remaining.Add(idRandomizeList[idRandom.Next(idRandomizeList.Count)]);
            }
            CurrentTurn = Remaining[0];
        }

        public event MBCEventHandler Event;

        /// <summary>
        /// A list of <see cref="Accolade"/>s that have been generated based on the currently generated
        /// <see cref="Event"/>s.
        /// </summary>
        public IEnumerable<Accolade> Accolades
        {
            get
            {
                return generatedAccolades;
            }
        }

        public virtual void End()
        {
            MakeEvent(new RoundEndEvent(this));
        }

        /// <summary>
        /// The <see cref="ControllerUser"/> that has the current turn.
        /// </summary>
        public ControllerID CurrentTurn
        {
            get;
            internal set;
        }

        /// <summary>
        /// Gets (or internally sets) a value indicating that this <see cref="Round"/> has ended and there can
        /// be no more <see cref="Event"/>s added.
        /// </summary>
        public bool Ended
        {
            get;
            internal set;
        }

        /// <summary>
        /// Gets the <see cref="MatchInfo"/> associated.
        /// </summary>
        public MatchInfo MatchInfo
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets a list of <see cref="ControllerRegister"/>s involved.
        /// </summary>
        public List<ControllerRegister> Registers
        {
            get;
            internal set;
        }

        /// <summary>
        /// The <see cref="ControllerUser"/>s that have not been defeated.
        /// </summary>
        public List<ControllerID> Remaining
        {
            get;
            private set;
        }

        /// <summary>
        /// Adds an <see cref="Accolade"/>. Mainly used by the internal <see cref="AccoladeGenerator"/>.
        /// </summary>
        /// <param name="accolade">The <see cref="Accolade"/> to add.</param>
        public void AddAccolade(Accolade accolade)
        {
            generatedAccolades.Add(accolade);
            MakeEvent(new RoundAccoladeEvent(this, accolade));
        }

        public bool StepBackward()
        {
            events.CurrentEvent.ProcBackward();
            Event(events.CurrentEvent, true);
            return events.StepBackward();
        }

        public bool StepForward()
        {
            if (Ended)
            {
                return true;
            }

            if (events.StepForward())
            {
                GameLogicStep();
            }
            else
            {
                events.CurrentEvent.ProcForward();
                Event(events.CurrentEvent, false);
            }
            return Ended;
        }

        /// <summary>
        /// Called when the <see cref="Round.CurrentState"/> is <see cref="State.Main"/>. Should continuously
        /// generate events until the game logic has determined that the <see cref="Round.CurrentState"/> should
        /// be changed to <see cref="State.End"/>.
        /// </summary>
        protected internal abstract void GameLogicStep();

        /// <summary>
        /// Creates a <see cref="RoundEvent"/> and invokes the class's <see cref="Round.RoundEvent"/>.
        /// </summary>
        /// <param name="ev">The <see cref="RoundEvent"/> created.</param>
        protected void MakeEvent(Event ev)
        {
            events.AddEvent(ev);
            ev.ProcForward();
            Event(ev, false);
        }
    }
}
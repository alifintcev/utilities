﻿// --------------------------------------------------------------------------------------------------------------------
// <copyright file="MailThrottle.cs" company="Lord Design">
//   (c) Lord Design. Modified GPL: You may use freely and commercially without modification; you can modify if result 
//   is also free.
// </copyright>
// <summary>
//   Multi-threaded mailer to keep your systems running by sending messages asynchronously. If you're using it in a
//   service that sends a lot of messages, be aware that you'll be limited by your .NET working threads.
// </summary>
// <author>aaron@lorddesign.net</author>
// --------------------------------------------------------------------------------------------------------------------
namespace LordDesign.Utilities
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading;

    /// <summary>
    /// The throttles.
    /// </summary>
    public class Throttles : IEachified<MailThrottle>
    {
        #region Constructors and Destructors

        /// <summary>
        /// Initializes a new instance of the <see cref="Throttles"/> class.
        /// </summary>
        public Throttles()
        {
            this.MinuteThrottle = new MailThrottle { Interval = ThrottleInterval.Minute, Limit = 180 };
            this.HourlyThrottle = new MailThrottle { Interval = ThrottleInterval.Hour, Limit = 3600 };
            this.DailyThrottle = new MailThrottle { Interval = ThrottleInterval.Day, Limit = 10000 };
        }

        #endregion

        #region Public Properties

        /// <summary>
        /// Gets the daily throttle.
        /// </summary>
        public MailThrottle DailyThrottle { get; private set; }

        /// <summary>
        /// Gets the hourly throttle.
        /// </summary>
        public MailThrottle HourlyThrottle { get; private set; }

        /// <summary>
        /// Gets the minute throttle.
        /// </summary>
        public MailThrottle MinuteThrottle { get; private set; }

        #endregion

        #region Public Methods and Operators

        /// <summary>
        /// The for each.
        /// </summary>
        /// <param name="func">
        /// The func.
        /// </param>
        public void ForEach(Action<MailThrottle> func)
        {
            func(this.MinuteThrottle);
            func(this.HourlyThrottle);
            func(this.DailyThrottle);
        }

        /// <summary>
        /// The increment.
        /// </summary>
        public void Increment()
        {
            this.ForEach(throttle => throttle.Increment());
        }

        /// <summary>
        /// The wait.
        /// </summary>
        /// <returns>
        /// The <see cref="bool"/>.
        /// </returns>
        public void Wait()
        {
            this.ForEach(
                x =>
                    {
                        while (x.Count() >= x.Limit)
                        {
                            Console.Write("Waiting...");
                            Thread.Sleep(100);
                        }
                    });
        }

        #endregion
    }

    /// <summary>
    /// The throttle interval.
    /// </summary>
    public enum ThrottleInterval
    {
        /// <summary>
        /// The minute.
        /// </summary>
        Minute, 

        /// <summary>
        /// The hour.
        /// </summary>
        Hour, 

        /// <summary>
        /// The day.
        /// </summary>
        Day
    }

    /// <summary>
    /// The mail throttle.
    /// </summary>
    public class MailThrottle
    {
        #region Fields

        /// <summary>
        /// The counter.
        /// </summary>
        private readonly List<DateTime> counter = new List<DateTime>();

        #endregion

        #region Public Properties

        /// <summary>
        /// Gets or sets the interval.
        /// </summary>
        public ThrottleInterval Interval { get; set; }

        /// <summary>
        /// Gets or sets the limit.
        /// </summary>
        public int Limit { get; set; }

        #endregion

        #region Public Methods and Operators

        /// <summary>
        /// The count.
        /// </summary>
        /// <returns>
        /// The <see cref="int"/>.
        /// </returns>
        public int Count()
        {
            DateTime startTime = this.GetStartTime();
            int count = this.counter.Count(x => x > startTime);
            this.Compact(startTime);
            return count;
        }

        /// <summary>
        /// The increment.
        /// </summary>
        public void Increment()
        {
            this.counter.Add(DateTime.Now);
            this.Compact();
        }

        #endregion

        #region Methods

        /// <summary>
        /// The compact.
        /// </summary>
        private void Compact()
        {
            this.Compact(this.GetStartTime());
        }

        /// <summary>
        /// The compact.
        /// </summary>
        /// <param name="startTime">
        /// The start time.
        /// </param>
        private void Compact(DateTime startTime)
        {
            this.counter.RemoveAll(x => x < startTime);
        }

        /// <summary>
        /// The get start time.
        /// </summary>
        /// <returns>
        /// The <see cref="DateTime"/>.
        /// </returns>
        /// <exception cref="System.ArgumentOutOfRangeException"></exception>
        private DateTime GetStartTime()
        {
            // Per minute 180, Per hour 3600, Per day 10,000
            // http://support.google.com/a/bin/answer.py?hl=en&answer=1366776
            switch (this.Interval)
            {
                case ThrottleInterval.Minute:
                    return DateTime.Now.AddMinutes(-1);
                case ThrottleInterval.Hour:
                    return DateTime.Now.AddHours(-1);
                case ThrottleInterval.Day:
                    return DateTime.Now.AddDays(-1);
            }

            throw new ArgumentOutOfRangeException();
        }

        #endregion
    }
}
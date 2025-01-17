﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;

namespace PRBD_Framework {
    public abstract class ObservableBase : ValidatableObjectBase, INotifyPropertyChanged, IDisposable {
        public event PropertyChangedEventHandler PropertyChanged;

        private bool _disposed;
        private List<string> _computedProps = new();

        public ObservableBase() {
            // pour les propriétés calculées
            PropertyChanged += (o, e) => {
                if (_computedProps.Contains(e.PropertyName)) return;
                foreach (var prop in _computedProps)
                    RaisePropertyChanged(prop);
            };
        }

        public void AddComputedProperties(params string[] props) {
            _computedProps.AddRange(props);
        }

        public void RemoveComputedProperties(params string[] props) {
            _computedProps = _computedProps.Where(p => !props.Contains(p)).ToList();
        }

        public void ClearComputedProperties() {
            _computedProps.Clear();
        }

        public void RaisePropertyChanged(string propertyName) {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public void RaisePropertyChanged(INotifyPropertyChanged source, string propertyName) {
            PropertyChanged?.Invoke(source, new PropertyChangedEventArgs(propertyName));
        }

        /// <summary>
		/// Checks if a property already matches a desired value. Sets the property and
		/// notifies listeners only when necessary. (origin: Prism)
		/// </summary>
		/// <typeparam name="T">Type of the property.</typeparam>
		/// <param name="storage">Reference to a property with both getter and setter.</param>
		/// <param name="value">Desired value for the property.</param>
		/// <param name="propertyName">Name of the property used to notify listeners. This
		/// value is optional and can be provided automatically when invoked from compilers that
		/// support CallerMemberName.</param>
		/// <returns>True if the value was changed, false if the existing value matched the
		/// desired value.</returns>
		protected virtual bool SetProperty<T>(ref T storage, T value, [CallerMemberName] string propertyName = null) {
            if (EqualityComparer<T>.Default.Equals(storage, value)) return false;

            storage = value;
            //Validate();
            RaisePropertyChanged(propertyName);

            return true;
        }

        protected bool SetProperty<TModel, T>(T oldValue, T newValue, TModel model, Action<TModel, T> callback, [CallerMemberName] string propertyName = null)
            where TModel : class {
            if (EqualityComparer<T>.Default.Equals(oldValue, newValue)) {
                return false;
            }

            callback(model, newValue);
            //Validate();
            RaisePropertyChanged(propertyName);

            return true;
        }

        /// <summary>
        /// Checks if a property already matches a desired value. Sets the property and
        /// notifies listeners only when necessary. (origin: Prism)
        /// </summary>
        /// <typeparam name="T">Type of the property.</typeparam>
        /// <param name="storage">Reference to a property with both getter and setter.</param>
        /// <param name="value">Desired value for the property.</param>
        /// <param name="propertyName">Name of the property used to notify listeners. This
        /// value is optional and can be provided automatically when invoked from compilers that
        /// support CallerMemberName.</param>
        /// <param name="onChanged">Action that is called after the property value has been changed.</param>
        /// <returns>True if the value was changed, false if the existing value matched the
        /// desired value.</returns>
        protected virtual bool SetProperty<T>(ref T storage, T value, Action onChanged, [CallerMemberName] string propertyName = null) {
            if (EqualityComparer<T>.Default.Equals(storage, value)) return false;

            storage = value;
            onChanged?.Invoke();
            //Validate();
            RaisePropertyChanged(propertyName);

            return true;
        }

        public void RaisePropertyChanged(params string[] propertyNames) {
            foreach (var n in propertyNames)
                RaisePropertyChanged(n);
        }

        public void RaisePropertyChanged(INotifyPropertyChanged source, params string[] propertyNames) {
            foreach (var n in propertyNames)
                RaisePropertyChanged(source, n);
        }

        /// <summary>
        /// Déclenche le PropertyChanged sur toutes les propriétés publiques.
        /// </summary>
        public void RaisePropertyChanged() {
            var type = GetType();
            foreach (var n in type.GetProperties())
                RaisePropertyChanged(n.Name);
        }

        public void RaisePropertyChanged(INotifyPropertyChanged source) {
            var type = GetType();
            foreach (var n in type.GetProperties())
                RaisePropertyChanged(source, n.Name);
        }

        //TODO: vérifier si bien implémenté : https://docs.microsoft.com/en-us/dotnet/standard/garbage-collection/implementing-dispose
        public virtual void Dispose() {
            if (!_disposed) {
                //Console.WriteLine("Disposing " + this);
                ApplicationRoot.UnRegister(this);

                // Supprime les bindings dont ce modèle de vue est la source ou la destination
                this.Unbind();

                _disposed = true;
                GC.SuppressFinalize(this);
            }
        }

        public void Register(Enum message, Action callback) {
            ApplicationRoot.Register(this, message, callback);
        }

        public void Register<T>(Enum message, Action<T> callback) {
            ApplicationRoot.Register(this, message, callback);
        }

        public static void NotifyColleagues(Enum message, object parameter) {
            ApplicationRoot.NotifyColleagues(message, parameter);
        }

        public static void NotifyColleagues(Enum message) {
            ApplicationRoot.NotifyColleagues(message);
        }

        public void UnRegister() {
            ApplicationRoot.UnRegister(this);
        }
    }
}

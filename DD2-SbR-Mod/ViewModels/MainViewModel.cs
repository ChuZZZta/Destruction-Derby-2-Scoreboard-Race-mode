﻿using Microsoft.Win32;
using Newtonsoft.Json;
using Sbr.Models;
using Sbr.ViewModels.Commands;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Threading;

namespace Sbr.ViewModels
{
    public class MainViewModel : INotifyPropertyChanged
    {
        //CTOR
        public MainViewModel()
        {
            dt = new DispatcherTimer
            {
                Interval = TimeSpan.FromSeconds(1)
            };
            dt.Tick += UpdateScore;
            this.SelectJsonCommand = new SelectJsonCommand(this);
            this.LoadConfigCommand = new LoadConfigCommand(this);
            GetRunningProcesses();
        }

        //Implemented intefaces for binding and command
        public event PropertyChangedEventHandler PropertyChanged;
        void OnPropertyChange(string propertyName)
        {
            if(PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }

        public LoadConfigCommand LoadConfigCommand { get; set; }
        public SelectJsonCommand SelectJsonCommand { get; set; }


        //Binded properties
        private string debugBox = "Debug Box";
        public string DebugBox{
            get { return debugBox; }
            set{
                debugBox = value;
                OnPropertyChange("DebugBox");
            }
        }
        private string jsonDriversPath = "Select a drivers config file";
        public string JsonDriversPath{
            get { return jsonDriversPath; }
            set{
                jsonDriversPath = value;
                OnPropertyChange("JsonDriversPath");
            }
        }
        private bool lapModeActive = false;
        public bool LapModeActive {
            get { return lapModeActive; }
            set {
                lapModeActive = value;
                OnPropertyChange("LapModeActive");
            }
        }
        private byte lapLimit = 0;
        public byte LapLimit {
            get { return lapLimit; }
            set {
                lapLimit = value;
                OnPropertyChange("LapLimit");
            }
        }
        private string selectedProcess = "";
        public string SelectedProcess
        {
            get { return selectedProcess; }
            set
            {
                selectedProcess = value;
                OnPropertyChange("SelectedProcess");
            }
        }
        private Map selectedMap;
        public Map SelectedMap
        {
            get { return selectedMap; }
            set
            {
                selectedMap = value;
                OnPropertyChange("SelectedMap");
            }
        }

        //Binded Lists

        public List<string> ProcessesList { get; set; } = new List<string>();
        public List<Map> MapList { get; set; } = new List<Map>();
        public List<Car> CarList { get; set; } = new List<Car>();

        //Variables

        DispatcherTimer dt;
        public string JsonMapsPath = "";
        string PicturesPath = "";

        //Commands

        public void SelectJson()
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            if (openFileDialog.ShowDialog() == true)
            {
                JsonDriversPath = openFileDialog.FileName;
                JsonMapsPath = Path.GetDirectoryName(JsonDriversPath) + "\\MapsInfo.json";
                PicturesPath = Path.GetDirectoryName(JsonDriversPath) + "\\Img\\test.jpg";
            }
        }

        public void LoadConfig()
        {
            GetCarInfo();
            GetTrackNames();
            dt.Start();
        }

        //Methods

        private void GetRunningProcesses()
        {
            Process[] processes = Process.GetProcesses();
            processes = processes.Where(x => x.SessionId != 0).ToArray();
            foreach (Process process in processes)
            {
                ProcessesList.Add(process.ProcessName);
            }
        }
        private void GetCarInfo()
        {
            Car[] cars = new Car[20];
            StreamReader sr = new StreamReader(JsonDriversPath);
            String json = sr.ReadToEnd();
            cars = JsonConvert.DeserializeObject<Car[]>(json);
            foreach (Car car in cars)
            {
                car.Processname = SelectedProcess;
                car.Picture = PicturesPath;
                CarList.Add(car);
            }
        }
        private void GetTrackNames()
        {
            
            Map[] maps = new Map[7];
            StreamReader sr = new StreamReader(JsonMapsPath);
            String json = sr.ReadToEnd();
            maps = JsonConvert.DeserializeObject<Map[]>(json);
            foreach (Map map in maps)
            {
                MapList.Add(map);
            }
            OnPropertyChange("MapList");
        }

        private void UpdateScore(object sender, EventArgs e)
        {
            foreach (Car car in CarList)
            {
                car.Update(LapLimit, LapModeActive, SelectedMap);
            }
            if (LapModeActive)
            {
                CarList = CarList.OrderByDescending(x => x.LapNumber).ThenBy(x => x.Position).ToList();
            }
            else
            {
                CarList = CarList.OrderBy(x => x.Position).ToList();
            }
            OnPropertyChange("CarList");
        }
    }
}

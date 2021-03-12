using System;
using System.Collections.Generic;
using System.Text;
using GeneticPcb.Models;
using ReactiveUI;

namespace GeneticPcb.ViewModels
{
    public class MainWindowViewModel : ViewModelBase
    {
        private PcbBoard _pcbBoard;

        public PcbBoard PcbBoard
        {
            get => _pcbBoard;
            set => this.RaiseAndSetIfChanged(ref _pcbBoard, value);
        }

        public MainWindowViewModel()
        {
            _pcbBoard = new PcbBoard(32, 24);
        }
    }
}
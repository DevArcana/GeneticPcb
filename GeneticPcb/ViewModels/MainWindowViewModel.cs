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
            var solderingPoints = new SolderingPoint[]
            {
                // new(0, 0),
                // new(1, 1),
                // new(2, 2),
                new(4, 16)
            };
            
            _pcbBoard = new PcbBoard(32, 24, solderingPoints);
        }
    }
}
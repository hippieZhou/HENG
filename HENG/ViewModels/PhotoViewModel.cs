﻿using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using Microsoft.Toolkit.Collections;
using Microsoft.Toolkit.Uwp;
using PixabaySharp.Models;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media.Animation;

namespace HENG.ViewModels
{
    public class PhotoViewModel : PixViewModel<PhotoItemSource, ImageItem>
    {
        public override ICommand LoadedCommand
        {
            get
            {
                if (_loadedCommand == null)
                {
                    _loadedCommand = new RelayCommand<object>(obj => 
                    {
                    });
                }
                return _loadedCommand;
            }
        }
        public override ICommand ItemClickCommand
        {
            get
            {
                if (_itemClickCommand == null)
                {
                    _itemClickCommand = new RelayCommand<ImageItem>(item => 
                    {
                        StoredItem = item;

                        if (StoredItem != null)
                        {
                            ConnectedAnimation animation = _listView.PrepareConnectedAnimation("forwardAnimation", StoredItem, "connectedElement");
                            animation.Completed += (sender, e) =>
                            {
                                var element = _listView.ContainerFromItem(StoredItem) as GridViewItem;
                                element.Opacity = 0d;
                            };
                            ViewModelLocator.Current.PhotoInfo.TryForwardStart(StoredItem, animation);
                        }
                    });
                }
                return _itemClickCommand;
            }
        }

        public async Task TryBackwardAsync(ImageItem storedItem, ConnectedAnimation animation)
        {
            StoredItem = storedItem;
            GridViewItem element = _listView.ContainerFromItem(StoredItem) as GridViewItem;
            element.Opacity = 1.0d;

            _listView.ScrollIntoView(StoredItem, ScrollIntoViewAlignment.Default);
            _listView.StartBringIntoView(new BringIntoViewOptions
            {
                AnimationDesired = true,
            });
            _listView.UpdateLayout();

            await _listView.TryStartConnectedAnimationAsync(animation, storedItem, "connectedElement");
        }
    }

    public class PhotoItemSource : IIncrementalSource<ImageItem>
    {
        public async Task<IEnumerable<ImageItem>> GetPagedItemsAsync(int pageIndex, int pageSize, CancellationToken cancellationToken = default)
        {
            var result = await ViewModelLocator.Current.Px.QueryImagesAsync(page: ++pageIndex, per_page: pageSize);
            await Task.Yield();
            return result?.Images;
        }
    }

    public class PixViewModel<TSource, IType> : ViewModelBase where TSource : IIncrementalSource<IType>
    {
        protected GridView _listView;

        private IType _storedItem;
        public IType StoredItem
        {
            get { return _storedItem; }
            protected set { Set(ref _storedItem, value); }
        }

        private IncrementalLoadingCollection<TSource, IType> _items;
        public IncrementalLoadingCollection<TSource, IType> Items
        {
            get { return _items; }
            set { Set(ref _items, value); }
        }

        private Visibility _headerVisibility = Visibility.Visible;
        public Visibility HeaderVisibility
        {
            get { return _headerVisibility; }
            set { Set(ref _headerVisibility, value); }
        }

        private Visibility _loadingVisibility = Visibility.Collapsed;
        public Visibility LoadingVisibility
        {
            get { return _loadingVisibility; }
            set { Set(ref _loadingVisibility, value); }
        }

        private Visibility _errorVisibility = Visibility.Collapsed;
        public Visibility ErrorVisibility
        {
            get { return _errorVisibility; }
            set { Set(ref _errorVisibility, value); }
        }

        public virtual void Initialize(GridView listView,int itemsPerPage = 20)
        {
            _listView = listView;

            if (Items == null)
            {
                Items = new IncrementalLoadingCollection<TSource, IType>(itemsPerPage,
                    async () =>
                    {
                        await Task.Delay(1000);

                        HeaderVisibility = Visibility.Collapsed;
                        LoadingVisibility = Visibility.Visible;
                        ErrorVisibility = Visibility.Collapsed;
                    },
                    () =>
                    {
                        LoadingVisibility = Visibility.Collapsed;
                    },
                    ex =>
                    {
                        ErrorVisibility = Visibility.Visible;
                    });
            };
        }

        protected ICommand _loadedCommand;
        public virtual ICommand LoadedCommand => _loadedCommand;

        protected ICommand _itemClickCommand;
        public virtual ICommand ItemClickCommand => _itemClickCommand;

        private ICommand _refreshCommand;
        public ICommand RefreshCommand
        {
            get
            {
                if (_refreshCommand == null)
                {
                    _refreshCommand = new RelayCommand(async () =>
                    {
                        await Items.RefreshAsync();
                    });
                }
                return _refreshCommand;
            }
        }
    }
}

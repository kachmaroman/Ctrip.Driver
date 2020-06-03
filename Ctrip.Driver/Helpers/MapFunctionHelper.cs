using System;
using System.Net.Http;
using System.Threading.Tasks;
using Android.Gms.Maps;
using Android.Gms.Maps.Model;
using Android.Graphics;
using Com.Google.Maps.Android;
using Java.Util;
using Newtonsoft.Json;

namespace Ctrip.Driver.Helpers
{
    public class MapFunctionHelper
    {
	    readonly string _mapkey;
	    readonly GoogleMap _mainMap;
        public Marker DestinationMarker;
        public Marker PositionMarker;

        bool _isRequestingDirection;

        public MapFunctionHelper(string mMapkey, GoogleMap mmap)
        {
            _mapkey = mMapkey;
            _mainMap = mmap;
        }

        public async Task<string> GetGeoJsonAsync(string url)
        {
	        HttpClientHandler handler = new HttpClientHandler();
            HttpClient client = new HttpClient(handler);

            return await client.GetStringAsync(url);
        }

        public async Task<string> GetDirectionJsonAsync(LatLng location, LatLng destination)
        {
	        string strOrigin = "origin=" + location.Latitude + "," + location.Longitude;

            string strDestination = "destination=" + destination.Latitude + "," + destination.Longitude;

            string mode = "mode=driving";

            string parameters = strOrigin + "&" + strDestination + "&" + "&" + mode + "&key=";

            string output = "json";

            string key = _mapkey;

            string url = "https://maps.googleapis.com/maps/api/directions/" + output + "?" + parameters + key;

            return await GetGeoJsonAsync(url);
        }

        public void DrawTripOnMap(string json)
        {
	        DirectionParser directionData = JsonConvert.DeserializeObject<DirectionParser>(json);

            var pointCode = directionData.routes[0].overview_polyline.points;
            var line = PolyUtil.Decode(pointCode);
            LatLng firstpoint = line[0];
            LatLng lastpoint = line[^1];

            //My take off position
            MarkerOptions markerOptions = new MarkerOptions();
            markerOptions.SetPosition(firstpoint);
            markerOptions.SetIcon(BitmapDescriptorFactory.DefaultMarker(BitmapDescriptorFactory.HueRed));
            _mainMap.AddMarker(markerOptions);

            //Constanly Changing Current Location;
            MarkerOptions positionMarkerOption = new MarkerOptions();
            positionMarkerOption.SetPosition(firstpoint);
            positionMarkerOption.SetTitle("Current Location");
            positionMarkerOption.SetIcon(BitmapDescriptorFactory.FromResource(Resource.Drawable.position));
            PositionMarker = _mainMap.AddMarker(positionMarkerOption);

            MarkerOptions markerOptions1 = new MarkerOptions();
            markerOptions1.SetPosition(lastpoint);
            markerOptions1.SetTitle("Pickup Location");
            markerOptions1.SetIcon(BitmapDescriptorFactory.DefaultMarker(BitmapDescriptorFactory.HueGreen));
            DestinationMarker = _mainMap.AddMarker(markerOptions1);

            ArrayList routeList = new ArrayList();

            foreach(LatLng item in line)
            {
                routeList.Add(item);
            }

            PolylineOptions polylineOptions = new PolylineOptions()
                .AddAll(routeList)
                .InvokeWidth(20)
                .InvokeColor(Color.Teal)
                .InvokeStartCap(new SquareCap())
                .InvokeEndCap(new SquareCap())
                .InvokeJointType(JointType.Round)
                .Geodesic(true);

            _mainMap.AddPolyline(polylineOptions);
            _mainMap.UiSettings.ZoomControlsEnabled = true;
            _mainMap.AnimateCamera(CameraUpdateFactory.NewLatLngZoom(firstpoint, 15));
            DestinationMarker.ShowInfoWindow();
        }

        public void DrawTripToDestination(string json)
        {
	        DirectionParser directionData = JsonConvert.DeserializeObject<DirectionParser>(json);

            var pointCode = directionData.routes[0].overview_polyline.points;
            var line = PolyUtil.Decode(pointCode);
            LatLng firstpoint = line[0];
            LatLng lastpoint = line[^1];

            //My take off position
            MarkerOptions markerOptions = new MarkerOptions();
            markerOptions.SetPosition(firstpoint);
            markerOptions.SetIcon(BitmapDescriptorFactory.DefaultMarker(BitmapDescriptorFactory.HueGreen));
            _mainMap.AddMarker(markerOptions);

            //Constanly Changing Current Location;
            MarkerOptions positionMarkerOption = new MarkerOptions();
            positionMarkerOption.SetPosition(firstpoint);
            positionMarkerOption.SetIcon(BitmapDescriptorFactory.FromResource(Resource.Drawable.position));
            PositionMarker = _mainMap.AddMarker(positionMarkerOption);

            MarkerOptions markerOptions1 = new MarkerOptions();
            markerOptions1.SetPosition(lastpoint);
            markerOptions1.SetIcon(BitmapDescriptorFactory.DefaultMarker(BitmapDescriptorFactory.HueRed));
            DestinationMarker = _mainMap.AddMarker(markerOptions1);

            ArrayList routeList = new ArrayList();
            
            foreach (LatLng item in line)
            {
                routeList.Add(item);
            }

            PolylineOptions polylineOptions = new PolylineOptions()
                .AddAll(routeList)
                .InvokeWidth(20)
                .InvokeColor(Color.Teal)
                .InvokeStartCap(new SquareCap())
                .InvokeEndCap(new SquareCap())
                .InvokeJointType(JointType.Round)
                .Geodesic(true);

            _mainMap.AddPolyline(polylineOptions);
            _mainMap.UiSettings.ZoomControlsEnabled = true;
            _mainMap.TrafficEnabled = true;

            LatLng southwest = new LatLng(directionData.routes[0].bounds.southwest.lat, directionData.routes[0].bounds.southwest.lng);
            LatLng northeast = new LatLng(directionData.routes[0].bounds.northeast.lat, directionData.routes[0].bounds.northeast.lng);

            LatLngBounds tripBounds = new LatLngBounds(southwest, northeast);
            _mainMap.AnimateCamera(CameraUpdateFactory.NewLatLngBounds(tripBounds, 100));
            DestinationMarker.ShowInfoWindow();
        }

        public async void UpdateMovement(LatLng myposition, LatLng destination, string whereto)
        {
            PositionMarker.Visible = true;
            PositionMarker.Position = myposition;

            if (!_isRequestingDirection)
            {
                _isRequestingDirection = true;
                string json = await GetDirectionJsonAsync(myposition, destination);
                DirectionParser directionData = JsonConvert.DeserializeObject<DirectionParser>(json);
                string duration = directionData.routes[0].legs[0].duration.text;
                PositionMarker.Title = "Current Location";
                PositionMarker.Snippet = duration + "Away from " + whereto;
                PositionMarker.ShowInfoWindow();
                _isRequestingDirection = false;
            }
        }

        public async Task<double> CalculateFares(LatLng firstpoint, LatLng lastpoint)
        {
            string directionJson = await GetDirectionJsonAsync(firstpoint, lastpoint);
            DirectionParser directionData = JsonConvert.DeserializeObject<DirectionParser>(directionJson);

            double distanceValue = directionData.routes[0].legs[0].distance.value;
            double durationValue = directionData.routes[0].legs[0].duration.value;

            const double minFare = 40;
            double baseFare = 30; //UAH 
            double distanceFare = 5; //UAH per kilometer
            double timeFare = 3; //UAH per minute

            double kmFare = (distanceValue / 1000) * distanceFare;
            double minsFare = (durationValue / 60) * timeFare;

            double amount = baseFare + kmFare + minsFare;
            double fare = Math.Floor(amount / 10) * 10;

            return fare < minFare ? minFare : fare;
        }
    }
}

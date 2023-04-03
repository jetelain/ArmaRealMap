/**
 *  Create a Canvas as ImageOverlay to draw the Lat/Lon Graticule,
 *  and show the axis tick label on the edge of the map.
 *  Author: lanwei@cloudybay.com.tw
 */
 
(function (window, document, undefined) {

    L.LatLngGraticule = L.Layer.extend({
        includes: L.Evented.prototype,

        options: {
            opacity: 1,
            weight: 0.8,
            color: '#444',
            font: '12px Verdana',
            zoomInterval: [
                { start: 0, end: 5, interval: 1000 }
            ]
        },

        initialize: function (options) {
            L.setOptions(this, options);

            var defaultFontName = 'Verdana';
            var _ff = this.options.font.split(' ');
            if (_ff.length < 2) {
                this.options.font += ' ' + defaultFontName;
            }

            if (!this.options.fontColor) {
                this.options.fontColor = this.options.color;
            }

            if (this.options.zoomInterval) {
                if (this.options.zoomInterval.latitude) {
                    this.options.latInterval = this.options.zoomInterval.latitude;
                    if (!this.options.zoomInterval.longitude) {
                        this.options.lngInterval = this.options.zoomInterval.latitude;
                    }
                }
                if (this.options.zoomInterval.longitude) {
                    this.options.lngInterval = this.options.zoomInterval.longitude;
                    if (!this.options.zoomInterval.latitude) {
                        this.options.latInterval = this.options.zoomInterval.longitude;
                    }
                }
                if (!this.options.latInterval) {
                    this.options.latInterval = this.options.zoomInterval;
                }
                if (!this.options.lngInterval) {
                    this.options.lngInterval = this.options.zoomInterval;
                }
            }
        },

        onAdd: function (map) {
            this._map = map;

            if (!this._container) {
                this._initCanvas();
            }

            map._panes.overlayPane.appendChild(this._container);

            map.on('viewreset', this._reset, this);
            map.on('move', this._reset, this);
            map.on('moveend', this._reset, this);

            this._reset();
        },

        onRemove: function (map) {
            map.getPanes().overlayPane.removeChild(this._container);

            map.off('viewreset', this._reset, this);
            map.off('move', this._reset, this);
            map.off('moveend', this._reset, this);
        },

        addTo: function (map) {
            map.addLayer(this);
            return this;
        },

        setOpacity: function (opacity) {
            this.options.opacity = opacity;
            this._updateOpacity();
            return this;
        },

        bringToFront: function () {
            if (this._canvas) {
                this._map._panes.overlayPane.appendChild(this._canvas);
            }
            return this;
        },

        bringToBack: function () {
            var pane = this._map._panes.overlayPane;
            if (this._canvas) {
                pane.insertBefore(this._canvas, pane.firstChild);
            }
            return this;
        },

        getAttribution: function () {
            return this.options.attribution;
        },

        _initCanvas: function () {
            this._container = L.DomUtil.create('div', 'leaflet-image-layer');

            this._canvas = L.DomUtil.create('canvas', '');

            if (this._map.options.zoomAnimation && L.Browser.any3d) {
                L.DomUtil.addClass(this._canvas, 'leaflet-zoom-animated');
            } else {
                L.DomUtil.addClass(this._canvas, 'leaflet-zoom-hide');
            }

            this._updateOpacity();

            this._container.appendChild(this._canvas);

            L.extend(this._canvas, {
                onselectstart: L.Util.falseFn,
                onmousemove: L.Util.falseFn,
                onload: L.bind(this._onCanvasLoad, this)
            });
        },

        _reset: function () {
            var container = this._container,
                canvas = this._canvas,
                size = this._map.getSize(),
                lt = this._map.containerPointToLayerPoint([0, 0]);

            L.DomUtil.setPosition(container, lt);

            container.style.width = size.x + 'px';
            container.style.height = size.y + 'px';

            canvas.width = size.x;
            canvas.height = size.y;
            canvas.style.width = size.x + 'px';
            canvas.style.height = size.y + 'px';

            this.__calcInterval();

            this.__draw(true);
        },

        _onCanvasLoad: function () {
            this.fire('load');
        },

        _updateOpacity: function () {
            L.DomUtil.setOpacity(this._canvas, this.options.opacity);
        },

        __format_lat: function (lat) {
            var str = "00" + (lat / 1000).toFixed();
            return str.substring(str.length - 2);
        },

        __format_lng: function (lng) {
            var str = "00" + (lng / 1000).toFixed();
            return str.substring(str.length - 2);
        },

        __calcInterval: function () {
            var zoom = this._map.getZoom();
            if (this._currZoom != zoom) {
                this._currLngInterval = 0;
                this._currLatInterval = 0;
                this._currZoom = zoom;
            }

            var interv;

            if (!this._currLngInterval) {
                try {
                    for (var idx in this.options.lngInterval) {
                        var dict = this.options.lngInterval[idx];
                        if (dict.start <= zoom) {
                            if (dict.end && dict.end >= zoom) {
                                this._currLngInterval = dict.interval;
                                break;
                            }
                        }
                    }
                }
                catch (e) {
                    this._currLngInterval = 0;
                }
            }

            if (!this._currLatInterval) {
                try {
                    for (var idx in this.options.latInterval) {
                        var dict = this.options.latInterval[idx];
                        if (dict.start <= zoom) {
                            if (dict.end && dict.end >= zoom) {
                                this._currLatInterval = dict.interval;
                                break;
                            }
                        }
                    }
                }
                catch (e) {
                    this._currLatInterval = 0;
                }
            }
        },

        __draw: function (label) {
            function _parse_px_to_int(txt) {
                if (txt.length > 2) {
                    if (txt.charAt(txt.length - 2) == 'p') {
                        txt = txt.substr(0, txt.length - 2);
                    }
                }
                try {
                    return parseInt(txt, 10);
                }
                catch (e) { }
                return 0;
            };

            var self = this,
                canvas = this._canvas,
                map = this._map

            if (L.Browser.canvas && map) {
                if (!this._currLngInterval || !this._currLatInterval) {
                    this.__calcInterval();
                }

                var latInterval = this._currLatInterval,
                    lngInterval = this._currLngInterval;

                var ctx = canvas.getContext('2d');
                ctx.clearRect(0, 0, canvas.width, canvas.height);
                ctx.lineWidth = this.options.weight;
                ctx.strokeStyle = this.options.color;
                ctx.fillStyle = this.options.fontColor;

                if (this.options.font) {
                    ctx.font = this.options.font;
                }
                var txtWidth = ctx.measureText('0').width;
                var txtHeight = 12;
                try {
                    var _font_size = ctx.font.split(' ')[0];
                    txtHeight = _parse_px_to_int(_font_size);
                }
                catch (e) { }

                var ww = canvas.width,
                    hh = canvas.height;

                var lt = map.containerPointToLatLng(L.point(0, 0));
                var rt = map.containerPointToLatLng(L.point(ww, 0));
                var rb = map.containerPointToLatLng(L.point(ww, hh));

                var _lat_b = rb.lat,
                    _lat_t = lt.lat;
                var _lon_l = lt.lng,
                    _lon_r = rt.lng;

                var _point_per_lat = (_lat_t - _lat_b) / (hh * 0.2);
                if (isNaN(_point_per_lat)) {
                    return;
                }

                if (_point_per_lat < 1) { _point_per_lat = 1; }
                _lat_b = parseInt(_lat_b - _point_per_lat, 10);
                _lat_t = parseInt(_lat_t + _point_per_lat, 10);
                var _point_per_lon = (_lon_r - _lon_l) / (ww * 0.2);
                if (_point_per_lon < 1) { _point_per_lon = 1; }
                _lon_r = parseInt(_lon_r + _point_per_lon, 10);
                _lon_l = parseInt(_lon_l - _point_per_lon, 10);

                var ll, latstr, lngstr;

                function __draw_lat_line(self, lat_tick) {
                    ll = self._latLngToCanvasPoint(L.latLng(lat_tick, _lon_l));
                    latstr = self.__format_lat(lat_tick);
                    txtWidth = ctx.measureText(latstr).width;

                    var __lon_right = _lon_r;
                    var rr = self._latLngToCanvasPoint(L.latLng(lat_tick, __lon_right));

                    /*ctx.beginPath();
                    ctx.moveTo(ll.x + 1, ll.y);
                    ctx.lineTo(rr.x - 1, rr.y);
                    ctx.stroke();*/

                    if (label) {
                        var _yy = ll.y + (txtHeight / 2) - 2;
                        ctx.fillText(latstr, 0, _yy);
                        ctx.fillText(latstr, ww - txtWidth, _yy);
                    }

                };

                if (latInterval > 0) {
                    for (var i = 0; i <= _lat_t; i += latInterval) {
                        if (i >= _lat_b) {
                            __draw_lat_line(this, i);
                        }
                    }
                }

                function __draw_lon_line(self, lon_tick) {
                    lngstr = self.__format_lng(lon_tick);
                    txtWidth = ctx.measureText(lngstr).width;
                    var bb = self._latLngToCanvasPoint(L.latLng(_lat_b, lon_tick));


                    var __lat_top = _lat_t;
                    var tt = self._latLngToCanvasPoint(L.latLng(__lat_top, lon_tick));

                    /*ctx.beginPath();
                    ctx.moveTo(tt.x, tt.y + 1);
                    ctx.lineTo(bb.x, bb.y - 1);
                    ctx.stroke();*/

                    if (label) {
                        ctx.fillText(lngstr, tt.x - (txtWidth / 2), txtHeight + 1);
                        ctx.fillText(lngstr, bb.x - (txtWidth / 2), hh - 3);
                    }

                };

                if (lngInterval > 0) {
                    for (var i = 0; i <= _lon_r; i += lngInterval) {
                        if (i >= _lon_l) {
                            __draw_lon_line(this, i);
                        }
                    }
                }
            }
        },

        _latLngToCanvasPoint: function (latlng) {
            map = this._map;
            var projectedPoint = map.project(L.latLng(latlng));
            projectedPoint._subtract(map.getPixelOrigin());
            return L.point(projectedPoint).add(map._getMapPanePos());
        }

    });

    L.latlngGraticule = function (options) {
        return new L.LatLngGraticule(options);
    };


}(this, document));

/**
 * Arma3 Maps data and utility functions
 *
 * Author: jetelain
 */
var Arma3Map = {

    Maps: {},
	
	toCoord: function (num, precision) {
		if (precision === undefined || precision > 5) {
			precision = 4;
		}
		if (num <= 0) {
			return '0'.repeat(precision);
		}
		var numText = "00000" + num.toFixed(0);
		return numText.substr(numText.length - 5, precision);
	},

	toGrid: function (latlng, precision) {
		return Arma3Map.toCoord(latlng.lng, precision) + " - " + Arma3Map.toCoord(latlng.lat, precision);
	},
	
	bearing : function (latlng1, latlng2) {
		return ((Math.atan2(latlng2.lng - latlng1.lng, latlng2.lat - latlng1.lat) * 180 / Math.PI) + 360) % 360;
	}

};

/**
 * Display mouse MGRS coordinates on map
 *
 * Author: jetelain
 */
L.Control.GridMousePosition = L.Control.extend({
  options: {
    position: 'topright',
	precision: 5
  },

  onAdd: function (map) {
    this._container = L.DomUtil.create('div', 'leaflet-grid-mouseposition');
    L.DomEvent.disableClickPropagation(this._container);
    map.on('mousemove', this._onMouseMove, this);
	var placeHolder = '0'.repeat(this.options.precision);
    this._container.innerHTML = placeHolder + ' - ' + placeHolder;
    return this._container;
  },

  onRemove: function (map) {
    map.off('mousemove', this._onMouseMove)
  },

  _onMouseMove: function (e) {
    this._container.innerHTML = Arma3Map.toGrid(e.latlng, this.options.precision);
  }

});

L.control.gridMousePosition = function (options) {
    return new L.Control.GridMousePosition(options);
};

function InitPreview(geoJson)
{
	const crs = L.extend({}, L.CRS.Simple, {
        projection: L.Projection.LonLat,
        transformation: new L.Transformation(0.1, 0, -0.1, 256),
        scale: function (zoom) {
            return Math.pow(2, zoom);
        },
        zoom: function (scale) {
            return Math.log(scale) / Math.LN2;
        },
        distance: function (latlng1, latlng2) {
            var dx = latlng2.lng - latlng1.lng,
                dy = latlng2.lat - latlng1.lat;
            return Math.sqrt(dx * dx + dy * dy);
        },
        infinite: true
    });
	
	const map = L.map('map', { minZoom: 0, maxZoom: 100, crs: crs });
	map.setView([500, 500], 3);
	L.latlngGraticule().addTo(map);
	L.control.scale({ maxWidth: 200, imperial: false }).addTo(map);
	L.control.gridMousePosition().addTo(map);
	
	L.geoJSON(geoJson, {
		style: function(feature) {
			switch (feature.properties.type) {
				case 'forest': return { fillColor:'ForestGreen', stroke: false, fillOpacity: 0.5 };
				case 'building': return { fillColor:'SaddleBrown', stroke: false, fillOpacity: 0.5 };
				case 'rocks': return { fillColor:'Black', stroke: false, fillOpacity: 0.5 };
				case 'scrub': return { fillColor:'SandyBrown', stroke: false, fillOpacity: 0.5 };
				case 'road': {
					switch (feature.properties.road) {
						case 'SingleLaneDirtPath': return { fillColor:'Indigo', stroke: false, fillOpacity: 0.5 };
					}
					return { fillColor:'Indigo', stroke: false, fillOpacity: 0.75 };
				}
				case 'lake': return { fillColor:'RoyalBlue', stroke: false, fillOpacity: 0.5 };
			}
			return { fillColor:'black', stroke: false, fillOpacity: 0.2 };
		}
	}).addTo(map);
}

import { useState } from 'react';
import { Card, Checkbox, Space, Tag, Typography } from 'antd';
import { useQuery } from '@tanstack/react-query';
import { MapContainer, TileLayer, CircleMarker, Popup } from 'react-leaflet';
import 'leaflet/dist/leaflet.css';
import {
  Cluster, CommerceLocation, getClusters, getCommerceLocations, getPetrolStations, PetrolStation,
} from '../api/client';

// Hưng Yên (merged province) approximate centre.
const CENTER: [number, number] = [20.72, 106.18];
const ALL = { page: 1, pageSize: 500 };

const layerMeta = {
  clusters: { label: 'Cụm công nghiệp', color: '#1677ff' },
  petrol: { label: 'Cửa hàng xăng dầu', color: '#fa8c16' },
  commerce: { label: 'Địa điểm thương mại', color: '#52c41a' },
} as const;

type LayerKey = keyof typeof layerMeta;

interface GeoPoint { id: string; lat: number; lng: number; title: string; subtitle: string; }

const withCoords = <T,>(items: T[] | undefined, map: (t: T) => GeoPoint | null): GeoPoint[] =>
  (items ?? []).map(map).filter((p): p is GeoPoint => p !== null);

export default function MapPage() {
  const [visible, setVisible] = useState<LayerKey[]>(['clusters', 'petrol', 'commerce']);

  const clusters = useQuery({ queryKey: ['map-clusters'], queryFn: () => getClusters(ALL) });
  const petrol = useQuery({ queryKey: ['map-petrol'], queryFn: () => getPetrolStations(ALL) });
  const commerce = useQuery({ queryKey: ['map-commerce'], queryFn: () => getCommerceLocations(ALL) });

  const points: Record<LayerKey, GeoPoint[]> = {
    clusters: withCoords(clusters.data?.items, (c: Cluster) =>
      c.latitude != null && c.longitude != null
        ? { id: c.id, lat: c.latitude, lng: c.longitude, title: c.name, subtitle: `Cụm CN · ${c.code}` } : null),
    petrol: withCoords(petrol.data?.items, (s: PetrolStation) =>
      s.latitude != null && s.longitude != null
        ? { id: s.id, lat: s.latitude, lng: s.longitude, title: s.name, subtitle: `Xăng dầu · ${s.code}` } : null),
    commerce: withCoords(commerce.data?.items, (l: CommerceLocation) =>
      l.latitude != null && l.longitude != null
        ? { id: l.id, lat: l.latitude, lng: l.longitude, title: l.name, subtitle: `Thương mại · ${l.code}` } : null),
  };

  return (
    <Space direction="vertical" style={{ width: '100%' }} size="middle">
      <Card size="small">
        <Space size="large" wrap>
          <Typography.Text strong>Bản đồ ngành Công Thương</Typography.Text>
          <Checkbox.Group
            value={visible}
            onChange={(v) => setVisible(v as LayerKey[])}
            options={(Object.keys(layerMeta) as LayerKey[]).map((k) => ({
              value: k,
              label: <Tag color={layerMeta[k].color}>{layerMeta[k].label} ({points[k].length})</Tag>,
            }))}
          />
        </Space>
      </Card>

      <div style={{ height: '72vh', width: '100%', borderRadius: 8, overflow: 'hidden' }}>
        <MapContainer center={CENTER} zoom={10} style={{ height: '100%', width: '100%' }} scrollWheelZoom>
          <TileLayer
            attribution='&copy; OpenStreetMap contributors'
            url="https://{s}.tile.openstreetmap.org/{z}/{x}/{y}.png"
          />
          {(Object.keys(layerMeta) as LayerKey[])
            .filter((k) => visible.includes(k))
            .flatMap((k) =>
              points[k].map((p) => (
                <CircleMarker key={`${k}-${p.id}`} center={[p.lat, p.lng]} radius={7}
                  pathOptions={{ color: layerMeta[k].color, fillColor: layerMeta[k].color, fillOpacity: 0.7 }}>
                  <Popup>
                    <b>{p.title}</b><br />{p.subtitle}
                  </Popup>
                </CircleMarker>
              )))}
        </MapContainer>
      </div>
    </Space>
  );
}

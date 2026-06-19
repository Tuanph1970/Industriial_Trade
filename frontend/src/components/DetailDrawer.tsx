import { ReactNode } from 'react';
import { Descriptions, Drawer } from 'antd';

export interface DetailItem { label: string; value: ReactNode; }

export interface DetailDrawerProps {
  open: boolean;
  onClose: () => void;
  title: string;
  items: DetailItem[];
}

/** Read-only record detail rendered as a labelled list in a right-side drawer. */
export default function DetailDrawer({ open, onClose, title, items }: DetailDrawerProps) {
  return (
    <Drawer open={open} onClose={onClose} title={title} width={520} destroyOnClose>
      <Descriptions column={1} bordered size="small"
        items={items.map((it, i) => ({
          key: i,
          label: it.label,
          children: it.value === null || it.value === undefined || it.value === '' ? '—' : it.value,
        }))} />
    </Drawer>
  );
}

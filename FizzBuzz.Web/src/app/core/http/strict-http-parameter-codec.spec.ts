import { HttpParams } from '@angular/common/http';
import { StrictHttpParameterCodec } from './strict-http-parameter-codec';

describe('StrictHttpParameterCodec', () => {
  const encode = (value: string) =>
    new HttpParams({ encoder: new StrictHttpParameterCodec(), fromObject: { q: value } }).toString();

  it('encodes characters that the default Angular codec leaves as is', () => {
    expect(encode('C++')).toBe('q=C%2B%2B');
    expect(encode('a=b;c/d')).toBe('q=a%3Db%3Bc%2Fd');
  });

  it('encodes separators, spaces and Unicode', () => {
    expect(encode('a & b')).toBe('q=a%20%26%20b');
    expect(encode('été😀')).toBe('q=%C3%A9t%C3%A9%F0%9F%98%80');
  });

  it('round-trips through decoding', () => {
    const codec = new StrictHttpParameterCodec();
    const original = 'C++ & co = 😀';

    expect(codec.decodeValue(codec.encodeValue(original))).toBe(original);
    expect(codec.decodeKey(codec.encodeKey(original))).toBe(original);
  });
});

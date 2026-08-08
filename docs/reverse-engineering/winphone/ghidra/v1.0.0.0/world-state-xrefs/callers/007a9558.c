
void FUN_007a9558(void)

{
  int iVar1;
  
  iVar1 = FUN_0058ef48();
  FUN_007a7bf4(0xb);
  if (*(char *)(iVar1 + 0x20) != '\0') {
    *(undefined1 *)(DAT_0107034c + 0x5cc5) = 0;
  }
  *(undefined1 *)(DAT_0107034c + 0x8e29) = 0;
  if (*(char *)(iVar1 + 0x20) != '\x01') {
    *(undefined1 *)(DAT_01070350 + 0x5cc5) = 0;
  }
  *(undefined1 *)(DAT_01070350 + 0x8e29) = 0;
  if (*(char *)(iVar1 + 0x20) != '\x02') {
    *(undefined1 *)(DAT_01070354 + 0x5cc5) = 0;
  }
  *(undefined1 *)(DAT_01070354 + 0x8e29) = 0;
  if (*(char *)(iVar1 + 0x20) != '\x03') {
    *(undefined1 *)(DAT_01070358 + 0x5cc5) = 0;
  }
  *(undefined1 *)(DAT_01070358 + 0x8e29) = 0;
  if (DAT_010980f8 == 0x34) {
    if (*(char *)(iVar1 + 0x2b42) == '\x01') {
      DAT_00fe1ec4 = 2;
    }
    FUN_006510f8();
  }
  DAT_010703d8 = DAT_01030850;
  DAT_010703dc = DAT_01030854;
  DAT_010703e0 = DAT_01030858;
  DAT_010703e1 = DAT_01030859;
  DAT_010703e2 = DAT_0103085a;
  DAT_010703e3 = DAT_0103085b;
  DAT_010703e4 = DAT_0103085c;
  DAT_010703e6 = DAT_0103085e;
  DAT_010703e8 = DAT_01030860;
  DAT_010703ea = DAT_01030862;
  DAT_010703ec = DAT_01030864;
  DAT_010703f0 = DAT_01030868;
  DAT_010703f4 = DAT_0103086c;
  DAT_010703f6 = DAT_0103086e;
  DAT_010703f8 = DAT_01030870;
  DAT_010703fc = DAT_01030874;
  DAT_01070400 = DAT_01030878;
  DAT_01070404 = DAT_0103087c;
  DAT_01070408 = DAT_01030880;
  DAT_0107040c = DAT_01030884;
  DAT_01070410 = DAT_01030888;
  DAT_01070414 = DAT_0103088c;
  DAT_01070418 = DAT_01030890;
  DAT_0107041c = DAT_01030894;
  FUN_0073ceb0(iVar1);
  FUN_006d8a34(*(undefined4 *)(iVar1 + 0x1c));
  *(undefined4 *)(iVar1 + 0x27e0) = 1;
  FUN_00592fc8();
  FUN_007a3c20();
  FUN_006415c0();
  DAT_01033984 = 1;
  return;
}


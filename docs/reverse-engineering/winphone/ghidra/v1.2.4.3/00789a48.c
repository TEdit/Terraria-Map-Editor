
void FUN_00789a48(void)

{
  int *piVar1;
  uint uVar2;
  int local_1c;
  undefined1 local_18 [4];
  
  piVar1 = (int *)FUN_0082a0b8();
  local_1c = 0x46;
  (**(code **)(*piVar1 + 0x20))(piVar1,&local_1c,4);
  local_1c = 0;
  (**(code **)(*piVar1 + 0x20))(piVar1,&local_1c,4);
  (**(code **)(*piVar1 + 0x40))(piVar1,&DAT_00a9b4d8);
  local_18[0] = DAT_00a592f6;
  (**(code **)(*piVar1 + 0x20))(piVar1,local_18,1);
  local_1c = FUN_00461c3c();
  FUN_00645dd0(&DAT_00ac34a4,&local_1c);
  if (100 < (uint)(DAT_00ac34a8 - (int)DAT_00ac34a4 >> 2)) {
    memmove(DAT_00ac34a4,(void *)((int)DAT_00ac34a4 + 4),
            (DAT_00ac34a8 - ((int)DAT_00ac34a4 + 4) >> 2) << 2);
    DAT_00ac34a8 = DAT_00ac34a8 + -4;
  }
  local_1c = DAT_00ac34a8 - (int)DAT_00ac34a4 >> 2;
  (**(code **)(*piVar1 + 0x20))(piVar1,&local_1c,4);
  uVar2 = 0;
  if (DAT_00ac34a8 - (int)DAT_00ac34a4 >> 2 != 0) {
    do {
      local_1c = *(int *)((int)DAT_00ac34a4 + uVar2 * 4);
      (**(code **)(*piVar1 + 0x20))(piVar1,&local_1c,4);
      uVar2 = uVar2 + 1;
    } while (uVar2 < (uint)(DAT_00ac34a8 - (int)DAT_00ac34a4 >> 2));
  }
  FUN_0082a0d0();
  return;
}


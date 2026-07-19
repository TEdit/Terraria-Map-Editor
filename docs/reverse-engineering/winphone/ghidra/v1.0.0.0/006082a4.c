
void FUN_006082a4(void)

{
  int iVar1;
  int *piVar2;
  int iVar3;
  int iVar4;
  undefined8 uVar5;
  char local_28 [4];
  
  uVar5 = FUN_007ffb80();
  iVar1 = DAT_0060832c;
  piVar2 = (int *)uVar5;
  iVar3 = 0;
  do {
    iVar4 = iVar3 + DAT_00fd643c;
    (**(code **)(*piVar2 + 0x1c))(piVar2,local_28,1);
    if (local_28[0] == '\0') {
      *(undefined1 *)(iVar4 + 0x1904) = 1;
    }
    else {
      FUN_00606b24(iVar4);
      *(undefined1 *)(iVar4 + 0x1904) = 0;
      (**(code **)(*piVar2 + 0x1c))(piVar2,iVar4 + 0x1900,2);
      (**(code **)(*piVar2 + 0x1c))(piVar2,iVar4 + 0x1902,2);
      FUN_00605a60(iVar4,piVar2,(int)((ulonglong)uVar5 >> 0x20));
    }
    iVar3 = iVar3 + 0x1908;
  } while (iVar3 < iVar1);
  FUN_007ffb98();
  return;
}


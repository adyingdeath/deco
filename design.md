## if

```
int a = 1;
if (a == 1) {
    print("true");
} else {
    print("false");
}
print("end");
```

```
block1 {
    int a = 1;
    if (a == 1) { call block2; }
    ifnot (a == 1) { call block3; }
    print("end");
}
block2 {
    print("true");
}
block3 {
    print("false");
}
```

## while

```
int a = 0;
while(a < 5) {
    print(a);
    if (a == 2) break;
    a++;
}
print("done");
```

```
block1 {
    int a = 0;
    if (a < 5) { function block2; }
    print("done");
}
block2 {
    print(a);
    if (a == 2) { return; }
    a++;
    if (a < 5) { function block2; }
}
```
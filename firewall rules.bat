netsh advfirewall firewall delete rule name="ShareP Service port" dir=in protocol=TCP localport=8000
netsh advfirewall firewall add rule name="ShareP Service port" dir=in action=allow protocol=TCP localport=8000

netsh advfirewall firewall delete rule name="ShareP Allow ICMP ping" protocol=icmpv4:8,any dir=in 
netsh advfirewall firewall add rule name="ShareP Allow ICMP ping" protocol=icmpv4:8,any dir=in action=allow
pause
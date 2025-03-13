import { Pagination } from "react-bootstrap";
import { MouseEventHandler } from "react";

export const pagerSize = 10;

export const Pager = ({
  page,
  totalPages,
  handlePageSelect,
  pageSize = 8
 }: { page: number, totalPages: number, handlePageSelect: (pageNumber: number) => MouseEventHandler, pageSize: number }) => {


  const pns = [];

  const pn = (i: number) => (
    <Pagination.Item key={i} active={i === page} onClick={handlePageSelect(i)}>
      {i}
    </Pagination.Item>
  );
  const el = (i: number) => (
    <Pagination.Ellipsis key={i} onClick={handlePageSelect(i)} />
  );
  const fst = () => (
    <Pagination.First key={1} active={1 === page} onClick={handlePageSelect(1)}>
      {1}
    </Pagination.First>
  );
  const lst = () => (
    <Pagination.Last
      key={totalPages}
      active={totalPages === page}
      onClick={handlePageSelect(totalPages)}
    >
      {totalPages}
    </Pagination.Last>
  );
  
  if (page <= pagerSize) {
    for (let i = 1; i <= pageSize; i++) {
      pns.push(pn(i));
    }
    pns.push(el(11));
    pns.push(lst());
  } else if (totalPages - page <= pagerSize) {
    pns.push(fst());
    pns.push(el(totalPages - 11));
    for (let i = totalPages - 10; i <= totalPages; i++) {
      pns.push(pn(i));
    }
  } else {
    pns.push(fst());
    pns.push(el(page - 4));
    for (let i = page - 3; i <= page + 3; i++) {
      pns.push(pn(i));
    }
    pns.push(el(page + 4));
    pns.push(lst());
  }

  return <Pagination>{pns}</Pagination>
  
};